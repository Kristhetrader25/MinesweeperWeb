using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents the Minesweeper game board.
    /// The Board class owns the 2D grid of <see cref="Cell"/> objects,
    /// manages bomb and reward placement, calculates neighbor counts,
    /// and tracks game metadata such as size, difficulty, timing, and state.
    /// </summary>
    public class Board
    {
       
        public int Size { get; set; }

     
        public float Difficulty { get; set; }

        public float Percentage { get; set; }

        
        public Cell[,] Cells { get; set; }

        
        public int RewardsRemaining { get; set; }

        
        public DateTime StartTime { get; set; }

        
        public DateTime EndTime { get; set; }

        
        public enum GameStatus { InProgress, Won, Lost }

        
        Random random = new Random();

        /// <summary>
        /// Creates a new board of the given size and difficulty.
        /// Allocates the grid and initializes bombs, rewards, and neighbor counts.
        /// </summary>
        /// <param name="size">Board dimension (board is size × size).</param>
        /// <param name="difficulty">Bomb probability between 0.0 and 0.25.</param>
        public Board(int size, float difficulty)
        {
            Size = size;
            Difficulty = difficulty;
            Cells = new Cell[size, size];
            RewardsRemaining = 0;
            InitializeBoard();
        }

        /// <summary>
        /// Initializes the board: places bombs, places rewards,
        /// calculates neighbor counts, and sets the game start time.
        /// </summary>
        private void InitializeBoard()
        {
            SetupBombs();
            SetupRewards();
            CalculateNumberOfBombNeighbors();
            StartTime = DateTime.Now;
        }

        
        /// <summary>
        /// Consumes a reward (if available) to undo a reveal at the given cell.
        /// This is the "mulligan" feature: it hides the cell again instead of ending the game.
        /// </summary>
        /// <param name="row">Row index of the target cell.</param>
        /// <param name="col">Column index of the target cell.</param>
        public void UseSpecialBonus(int row, int col)
        {
            if (RewardsRemaining > 0 && IsCellOnBoard(row, col))
            {
                Cell cell = Cells[row, col];
                if (cell.IsRevealed)
                {
                    if (cell.UndoReveal())
                    {
                        RewardsRemaining--;
                        Console.WriteLine($"Undo used at ({row},{col}). Rewards left: {RewardsRemaining}");
                    }
                }
            }
        }

        
        /// <summary>
        /// Calculates the final score after the game ends.
        /// Currently a stub. Later versions should factor in difficulty,
        /// elapsed time, board size, and rewards used.
        /// </summary>
        /// <returns>Score as an integer (currently always 0).</returns>
        public int DetermineFinalScore()
        {
            return 0;
        }

        public void FloodFill(int row, int col)
        {
            if (!IsCellOnBoard(row, col)) return;

            var cell = Cells[row, col];

            // Do not process bombs or already revealed/flagged cells
            if (cell.Live || cell.IsRevealed || cell.IsFlagged) return;

            // Reveal this safe cell
            bool safe = cell.Reveal(); // true because we checked Live above
            if (!safe) return;

            // Collect reward if present
            if (cell.CollectReward())
            {
                RewardsRemaining++;
                // Console message is fine for console version; GUI ignores it
                Console.WriteLine($"A reward was collected! Rewards now: {RewardsRemaining}");
            }

            // If numbered, we stop (boundary)
            if (cell.LiveNeighbors > 0) return;

            // Otherwise recurse into neighbors
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    FloodFill(row + dr, col + dc);
                }
            }
        }

        /// <summary>
        /// Helper function to determine if a cell is out of bounds
        /// Returns true if the given row and column indices are within board boundaries.
        /// </summary>
        private bool IsCellOnBoard(int row, int col)
        {
            return row >= 0 && row < Size && col >= 0 && col < Size;
        }

        
        /// <summary>
        /// For each cell in the grid, calculate how many bombs surround it.
        /// Bomb cells are assigned a special value of 9 in <see cref="Cell.LiveNeighbors"/>.
        /// </summary>
        private void CalculateNumberOfBombNeighbors()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Cells[i, j].Live)
                    {
                        // Convention: bombs use neighbor count of 9
                        Cells[i, j].LiveNeighbors = 9;
                    }
                    else
                    {
                        Cells[i, j].LiveNeighbors = GetNumberOfBombNeighbors(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Helper function to determine the number of bomb neighbors for a cell
        /// Counts how many bombs surround the given cell by checking
        /// all 8 neighboring positions using delta-row (dr) and delta-column (dc).
        /// </summary>
        /// <param name="i">Row index of the cell.</param>
        /// <param name="j">Column index of the cell.</param>
        /// <returns>The number of bomb neighbors (0–8).</returns>
        private int GetNumberOfBombNeighbors(int i, int j)
        {
            int count = 0;

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    // Skip the cell itself
                    if (dr == 0 && dc == 0) continue;

                    int r = i + dr;
                    int c = j + dc;

                    // If neighbor is on the board and live, increment count
                    if (IsCellOnBoard(r, c) && Cells[r, c].Live)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

       
        /// <summary>
        /// Initializes the grid and assigns bombs based on difficulty probability.
        /// First creates non-live cells, then assigns bombs independently
        /// with probability p (clamped between 0 and 0.25).
        /// </summary>
        private void SetupBombs()
        {
            // Initialize all cells first
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Cells[i, j] = new Cell
                    {
                        Live = false,
                        LiveNeighbors = 0,
                        HasReward = false
                    };
                }
            }

            // We interpret Difficulty as a probability (0.0–0.25).
            float p = Math.Max(0f, Math.Min(0.25f, Difficulty));

            // Assign bombs independently by probability p
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (random.NextDouble() < p)
                    {
                        Cells[i, j].Live = true;
                    }
                }
            }
        }


        /// <summary>
        /// Randomly places a small number of rewards on the grid.
        /// Rewards never overlap bombs and are guaranteed to place at least one.
        /// </summary>
        // Use during setup to place rewards on the board
        private void SetupRewards()
        {
            // Build a list of all safe (non-bomb) cells
            var safeCells = new List<(int r, int c)>();
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    Cells[r, c].HasReward = false; 
                    if (!Cells[r, c].Live)
                        safeCells.Add((r, c));
                }
            }

            // If (pathologically) there are no safe cells, force one by demoting a random bomb.
            // This guarantees at least one reward can be placed.
            if (safeCells.Count == 0)
            {
                int rr = random.Next(Size);
                int cc = random.Next(Size);
                Cells[rr, cc].Live = false;   
                safeCells.Add((rr, cc));
                
            }

            int totalCells = Size * Size;

            // Target ≈ 2% of cells as rewards, but:
            // - at least 1,
            // - at most number of safe cells (can't place rewards on bombs).
            int target = (int)Math.Round(totalCells * 0.02);
            target = Math.Max(1, Math.Min(target, safeCells.Count));

            // Shuffle safe cells and take the first 'target' positions
            for (int i = safeCells.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (safeCells[i], safeCells[j]) = (safeCells[j], safeCells[i]);
            }

            for (int k = 0; k < target; k++)
            {
                var (r, c) = safeCells[k];
                Cells[r, c].HasReward = true;
            }
        }



        /// <summary>
        /// Determines the current game state by scanning all cells.
        /// <list type="bullet">
        ///  <item><description><b>Lost</b> if any bomb cell has been revealed.</description></item>
        ///  <item><description><b>Won</b> when every cell is resolved:
        ///  bombs are correctly flagged and all non-bomb cells are revealed.</description></item>
        ///  <item><description><b>InProgress</b> otherwise (there is at least one unresolved cell).</description></item>
        ///  </list>
        /// </summary>
        /// <returns>A <see cref="GameStatus"/> value indicating Lost, Won, or InProgress.</returns>
        public GameStatus DetermineGameState()
        {
            bool anyUnresolved = false;

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    var cell = Cells[r, c];

                    // If a bomb was revealed, lost
                    if (cell.Live && cell.IsRevealed)
                        return GameStatus.Lost;

                    // Resolved if: bombs are flagged OR safe cells are revealed
                    bool resolved =
                        (cell.Live && cell.IsFlagged) ||
                        (!cell.Live && cell.IsRevealed);

                    if (!resolved) anyUnresolved = true;
                }
            }

            return anyUnresolved ? GameStatus.InProgress : GameStatus.Won;
        }


        /*/// <summary>
        /// Returns the current game status.
        /// Currently always returns <see cref="GameStatus.InProgress"/>.
        /// Future versions will detect win/loss conditions.
        /// </summary>
        public GameStatus DetermineGameState()
        {
            return GameStatus.InProgress;
        }*/
    }
}
