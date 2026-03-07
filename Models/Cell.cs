using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents a single cell (square) on the Minesweeper board.
    /// A Cell can contain a bomb, track the number of bomb neighbors,
    /// hold a reward, and store its current player-visible state
    /// (revealed, hidden, or flagged).
    /// </summary>
    public class Cell
    {
        
        public bool Live { get; set; }

        
        public int LiveNeighbors { get; set; }

        
        public bool HasReward { get; set; }

        
        public bool IsRevealed { get; private set; }

        
        public bool IsFlagged { get; private set; }

        /// <summary>
        /// Default constructor. Creates a hidden, safe cell with no reward and no flags.
        /// </summary>
        public Cell()
        {
            Live = false;
            LiveNeighbors = 0;
            HasReward = false;
            IsRevealed = false;
            IsFlagged = false;
        }

        /// <summary>
        /// Reveals the cell. Returns false if the cell contains a bomb, true if it is safe.
        /// If the cell is already revealed, calling again returns the same outcome.
        /// </summary>
        /// <returns>True if safe, false if bomb.</returns>
        public bool Reveal()
        {
            if (IsRevealed)
            {
                // Already revealed: just return its status
                return !Live;
            }
            else
            {
                IsRevealed = true;
                return !Live;
            }
        }

        /// <summary>
        /// Undoes a reveal by hiding the cell again.
        /// Used when the player spends a reward (mulligan).
        /// </summary>
        /// <returns>True if undo succeeded; false if cell was not revealed.</returns>
        public bool UndoReveal()
        {
            if (IsRevealed)
            {
                IsRevealed = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Toggles the flagged state (suspected bomb) if the cell is not revealed.
        /// </summary>
        public void ToggleFlag()
        {
            if (!IsRevealed)
            {
                IsFlagged = !IsFlagged;
            }
        }

        /// <summary>
        /// Collects the reward if the cell is revealed and has one.
        /// Consumes the reward and returns true if successful.
        /// </summary>
        /// <returns>True if reward was collected; false otherwise.</returns>
        public bool CollectReward()
        {
            if (IsRevealed && HasReward)
            {
                HasReward = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the "answer key" character for this cell:
        /// 'B' if bomb, '.' if no neighbors, or digit ('1'..'8') for neighbor counts.
        /// 'R' if reward. 
        /// </summary>
        /// <returns>Character representing the true content of this cell.</returns>
        public char GetAnswerChar()
        {
            if (Live) return 'B';
            if (HasReward) return 'R';
            return LiveNeighbors == 0 ? '.' : (char)('0' + Math.Min(LiveNeighbors, 9));
        }

        /// <summary>
        /// Gets the "visible" character for the player’s view:
        /// '.' for unrevealed, 'F' if flagged, 'B' if revealed bomb,
        /// digit ('1'..'8') if revealed with neighbors, or blank if zero neighbors.
        /// </summary>
        /// <returns>Character representing what the player currently sees.</returns>
        public char GetVisibleChar()
        {
            if (!IsRevealed) return IsFlagged ? 'F' : '.';
            if (Live) return 'B';
            return LiveNeighbors == 0 ? ' ' : (char)('0' + Math.Min(LiveNeighbors, 9));
        }

        /// <summary>
        /// Returns the string form of the cell, which matches its visible character.
        /// </summary>
        public override string ToString() => GetVisibleChar().ToString();


        /// <summary>
        /// Sets the full state without triggering side effects (used by load/resume).
        /// </summary>
        public void SetState(bool live, int liveNeighbors, bool hasReward, bool isRevealed, bool isFlagged)
        {
            Live = live;
            LiveNeighbors = liveNeighbors;
            HasReward = hasReward;

            // Directly set reveal/flag states to avoid reward collection or loss triggers.
            IsRevealed = isRevealed;
            IsFlagged = isFlagged;
        }

    }
}
