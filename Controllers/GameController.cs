using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MinesweeperWeb.Controllers
{
    /// <summary>
    /// Handles game-related pages for the Minesweeper application.
    /// </summary>
    public class GameController : Controller
    {
        /// <summary>
        /// Stores a Minesweeper board instance for each logged-in user.
        /// 
        /// Key:
        ///     UserId stored in session.
        /// 
        /// Value:
        ///     The Board object representing that user's current game.
        /// 
        /// This allows multiple users to play independent games simultaneously
        /// without sharing the same board instance.
        /// </summary>
        private static readonly Dictionary<int, Board> _boards = new Dictionary<int, Board>();

        /// <summary>
        /// Session key used to allow showing the finished board one time
        /// when the user clicks "Back to Board" from Win/Loss.
        /// </summary>
        private const string AllowFinishedBoardSessionKey = "AllowFinishedBoard";

        /// <summary>
        /// Stores the final score in session so it is available across redirects/pages.
        /// </summary>
        private void SaveFinalScoreToSession(int score)
        {
            HttpContext.Session.SetInt32("FinalScore", score);
        }

        /// <summary>
        /// Reads the final score from session. Returns 0 if not present.
        /// </summary>
        private int GetFinalScoreFromSession()
        {
            return HttpContext.Session.GetInt32("FinalScore") ?? 0;
        }

        /// <summary>
        /// Feature toggle:
        /// Enable or disable right click.
        /// </summary>
        private const bool EnableRightClickFlagging = true;

        /// <summary>
        /// Converts the difficulty string stored in session into a bomb probability (0.0–0.25).
        /// Supports common labels like Easy/Medium/Hard or numeric text like "0.10".
        /// </summary>
        /// <param name="difficultyText">Difficulty label or numeric text.</param>
        /// <returns>Bomb probability float clamped between 0.0 and 0.25.</returns>
        private static float ParseDifficulty(string difficultyText)
        {
            if (string.IsNullOrWhiteSpace(difficultyText))
            {
                return 0.10f; // safe default
            }

            string d = difficultyText.Trim().ToLowerInvariant();

            // Common label support (can adjust if needed)
            if (d.Contains("easy")) return 0.05f;
            if (d.Contains("medium")) return 0.12f;
            if (d.Contains("hard")) return 0.18f;

            // Numeric support: "0.10", "0.15", etc.
            if (float.TryParse(difficultyText, out float parsed))
            {
                // Clamp to the board class expectation (0.0–0.25)
                return Math.Max(0f, Math.Min(0.25f, parsed));
            }

            return 0.10f;
        }

        /// <summary>
        /// Builds a list of UI button models from the current board cells.
        /// Each UI button maps to exactly one Cell in the board.
        /// </summary>
        /// <param name="board">The board to convert to UI models.</param>
        /// <returns>A list of CellButtonModel objects for rendering in the view.</returns>
        private static List<CellButtonModel> BuildButtonsFromBoard(Board board)
        {
            var buttons = new List<CellButtonModel>(board.Size * board.Size);

            for (int r = 0; r < board.Size; r++)
            {
                for (int c = 0; c < board.Size; c++)
                {
                    int id = (r * board.Size) + c;
                    Cell cell = board.Cells[r, c];

                    // Default: hidden tile image until revealed.
                    string imageName = "Tile 1.png";
                    string altText = "Hidden cell";


                    /// Determine which image should represent the cell
                    /// based on its current state.
                    if (EnableRightClickFlagging && cell.IsFlagged)
                    {
                        imageName = "Flag.png";
                        altText = "Flagged bomb location";
                    }
                    else if (cell.IsRevealed)
                    {
                        if (cell.Live)
                        {
                            imageName = "Skull.png";
                            altText = "Bomb";
                        }
                        else if (cell.HasReward)
                        {
                            imageName = "Gold.png";
                            altText = "Reward";
                        }
                        else if (cell.LiveNeighbors == 0)
                        {
                            imageName = "Tile Flat.png";
                            altText = "Empty cell";
                        }
                        else
                        {
                            imageName = $"Number {cell.LiveNeighbors}.png";
                            altText = $"Number {cell.LiveNeighbors}";
                        }
                    }

                    buttons.Add(new CellButtonModel
                    {
                        Id = id,
                        Row = r,
                        Col = c,
                        ImageName = imageName,
                        AltText = altText
                    });
                }
            }

            return buttons;
        }

        /// <summary>
        /// Calculates the player's final score after the game ends.
        /// 
        /// The score is based on three main factors:
        /// 1) Board size (larger boards yield higher base points)
        /// 2) Difficulty level (higher bomb probability increases score multiplier)
        /// 3) Time taken to finish the game (faster completion results in higher scores) 
        /// 
        /// </summary>
        /// <param name="board">The completed Minesweeper board.</param>
        /// <returns>An integer representing the player's final score.</returns>
        private static int CalculateScore(Board board)
        {
            // EndTime should be set when the game ends (won/lost).
            // If it is not set yet, use current time.
            DateTime end = board.EndTime == default ? DateTime.Now : board.EndTime;

            // Total seconds taken.
            double seconds = (end - board.StartTime).TotalSeconds;
            if (seconds < 1) seconds = 1;

            // Base points scale with number of cells.
            int basePoints = board.Size * board.Size * 10;

            // Difficulty multiplier based on bomb probability (0.10, 0.15, 0.20).
            // This keeps it aligned to your dropdown mapping.
            double difficultyMultiplier = 1.0 + (board.Difficulty * 4.0);  // ex: 0.10 -> 1.4, 0.20 -> 1.8

            // Time penalty so faster completion yields a higher score.
            double timePenalty = seconds;

            int score = (int)Math.Max(0, (basePoints * difficultyMultiplier) - timePenalty);

            return score;
        }

        /// <summary>
        /// Displays the StartGame page only if the user is logged in.
        /// This page allows the player to choose a board size and difficulty.
        /// </summary>
        /// <returns>StartGame view with default selections if authenticated; otherwise redirects to Login.</returns>
        [HttpGet]
        public IActionResult StartGame()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Provide default values for the StartGame form.
            StartGameViewModel model = new StartGameViewModel();
            return View(model);
        }

        /// <summary>
        /// Processes the StartGame form submission.
        /// Stores the player's game settings in session and resets any previous board.
        /// </summary>
        /// <param name="model">The board size and difficulty selected by the user.</param>
        /// <returns>Redirects to MineSweeperBoard if valid; otherwise redisplays StartGame.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartGame(StartGameViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userKey = userId.Value;

            // Store game settings in session
            HttpContext.Session.SetInt32("BoardSize", model.BoardSize);
            HttpContext.Session.SetString("Difficulty", model.Difficulty);

            /// <summary>
            /// Remove any previous board for this user.
            /// </summary>
            if (_boards.ContainsKey(userKey))
            {
                _boards.Remove(userKey);
            }

            // Clear any stored score
            HttpContext.Session.Remove("FinalScore");

            // Clear any one-time "Back to Board" flag so a new game behaves normally.
            HttpContext.Session.Remove(AllowFinishedBoardSessionKey);

            return RedirectToAction(nameof(MineSweeperBoard));
        }

        /// <summary>
        /// Displays the MineSweeperBoard page for the logged-in user.
        /// </summary>
        [HttpGet]
        public IActionResult MineSweeperBoard()
        {
            // Restrict access: user must be logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userKey = userId.Value;

            // Require game settings to exist.
            int? boardSize = HttpContext.Session.GetInt32("BoardSize");
            string? difficultyText = HttpContext.Session.GetString("Difficulty");

            if (boardSize == null || string.IsNullOrWhiteSpace(difficultyText))
            {
                return RedirectToAction(nameof(StartGame));
            }

            // If no board exists for this user yet, create one.
            if (!_boards.ContainsKey(userKey))
            {
                float difficulty = ParseDifficulty(difficultyText);
                _boards[userKey] = new Board(boardSize.Value, difficulty);
            }

            Board board = _boards[userKey];

            // Determine current status.
            Board.GameStatus status = board.DetermineGameState();
            ViewBag.GameStatus = status;

            // If the game ended, normally redirect to the outcome page.
            // Exception: allow rendering the finished board one time after clicking "Back to Board".
            bool allowFinishedBoard = HttpContext.Session.GetInt32(AllowFinishedBoardSessionKey) == 1;

            if (status == Board.GameStatus.Won && !allowFinishedBoard)
            {
                return RedirectToAction(nameof(Win));
            }

            if (status == Board.GameStatus.Lost && !allowFinishedBoard)
            {
                return RedirectToAction(nameof(Loss));
            }

            // If allowing it, consume the one-time flag so refresh doesn't keep bypassing redirects.
            if (allowFinishedBoard)
            {
                HttpContext.Session.Remove(AllowFinishedBoardSessionKey);
            }

            // Build the UI button list.
            List<CellButtonModel> model = BuildButtonsFromBoard(board);

            // Keep showing the settings on the page.
            ViewBag.BoardSize = board.Size;
            ViewBag.Difficulty = difficultyText;

            return View(model);
        }

        /// <summary>
        /// Handles a left-click on a cell button.
        /// Reveals the clicked cell, applies flood fill for empty cells,
        /// and redirects back to the board so the updated state is shown.
        /// </summary>
        /// <param name="id">Flattened cell id (0..Size*Size-1).</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CellClick(int id)
        {
            // Restrict access: user must be logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userKey = userId.Value;

            // If the user does not have a board initialized yet, redirect them back to StartGame.
            if (!_boards.ContainsKey(userKey))
            {
                return RedirectToAction(nameof(StartGame));
            }

            Board board = _boards[userKey];

            // Prevent any further clicks once the game is no longer in progress.
            if (board.DetermineGameState() != Board.GameStatus.InProgress)
            {
                return RedirectToAction(nameof(MineSweeperBoard));
            }

            // Convert flattened id -> row/col.
            int row = id / board.Size;
            int col = id % board.Size;

            // Safety: validate bounds.
            if (row < 0 || row >= board.Size || col < 0 || col >= board.Size)
            {
                return RedirectToAction(nameof(MineSweeperBoard));
            }

            Cell cell = board.Cells[row, col];

            // Left-click behavior:
            // - Do nothing if flagged or already revealed
            // - Reveal the cell otherwise
            // - Flood fill if the cell has zero neighbors
            if (!cell.IsFlagged && !cell.IsRevealed)
            {
                bool safe = cell.Reveal();

                // If it is safe and empty, reveal neighbors.
                if (safe && cell.LiveNeighbors == 0)
                {
                    board.FloodFill(row, col);
                }

                // If a reward exists and the cell was revealed, collect it and increase the count.
                if (cell.CollectReward())
                {
                    board.RewardsRemaining++;
                }
            }

            // After the move completes, check the game state to determine whether the player won or lost.
            Board.GameStatus status = board.DetermineGameState();

            // Persist the updated board state for this user.
            _boards[userKey] = board;

            // If the game just ended, capture end time and route to the appropriate outcome page.
            if (status != Board.GameStatus.InProgress)
            {
                // Record when the game ended (used by scoring).
                board.EndTime = DateTime.Now;

                // Calculate and persist the final score.
                int score = CalculateScore(board);
                SaveFinalScoreToSession(score);

                // Persist the finished board state for this user.
                _boards[userKey] = board;

                // Redirect to the appropriate outcome page.
                if (status == Board.GameStatus.Won)
                {
                    return RedirectToAction(nameof(Win));
                }

                return RedirectToAction(nameof(Loss));
            }

            // Otherwise continue playing.
            return RedirectToAction(nameof(MineSweeperBoard));
        }

        /// <summary>
        /// Handles a right-click on a cell button.
        /// Toggles the flagged state of the selected cell.
        /// </summary>
        /// <param name="id">Flattened cell id (0..Size*Size-1).</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFlag(int id)
        {
            // Restrict access: user must be logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Milestone 2: right-click flagging disabled.
            if (!EnableRightClickFlagging)
            {
                return RedirectToAction(nameof(MineSweeperBoard));
            }

            int userKey = userId.Value;

            if (!_boards.ContainsKey(userKey))
            {
                return RedirectToAction(nameof(StartGame));
            }

            Board board = _boards[userKey];

            // Prevent flag changes once the game is no longer in progress.
            if (board.DetermineGameState() != Board.GameStatus.InProgress)
            {
                return RedirectToAction(nameof(MineSweeperBoard));
            }

            // Convert id -> row/col
            int row = id / board.Size;
            int col = id % board.Size;

            if (row < 0 || row >= board.Size || col < 0 || col >= board.Size)
            {
                return RedirectToAction(nameof(MineSweeperBoard));
            }

            Cell cell = board.Cells[row, col];

            // Toggle flag only if cell has not been revealed.
            if (!cell.IsRevealed)
            {
                cell.ToggleFlag();
            }

            // Persist updated board state.
            _boards[userKey] = board;

            /// <summary>
            /// After toggling a flag, re-check the game state.
            /// </summary>
            Board.GameStatus status = board.DetermineGameState();

            if (status != Board.GameStatus.InProgress)
            {
                // Record when the game ended (used by scoring).
                board.EndTime = DateTime.Now;

                // Calculate and persist the final score.
                int score = CalculateScore(board);
                SaveFinalScoreToSession(score);

                // Persist the finished board state for this user.
                _boards[userKey] = board;

                // Redirect to the appropriate outcome page.
                if (status == Board.GameStatus.Won)
                {
                    return RedirectToAction(nameof(Win));
                }

                return RedirectToAction(nameof(Loss));
            }

            return RedirectToAction(nameof(MineSweeperBoard));
        }

        /// <summary>
        /// Allows the user to view the finished board one time after Win/Loss.
        /// This prevents MineSweeperBoard from immediately redirecting back to the outcome page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BackToBoard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Set a one-time flag that MineSweeperBoard will consume.
            HttpContext.Session.SetInt32(AllowFinishedBoardSessionKey, 1);

            return RedirectToAction(nameof(MineSweeperBoard));
        }

        /// <summary>
        /// Restarts the current game using the existing session settings.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restart()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userKey = userId.Value;

            int? boardSize = HttpContext.Session.GetInt32("BoardSize");
            string? difficultyText = HttpContext.Session.GetString("Difficulty");

            if (boardSize == null || string.IsNullOrWhiteSpace(difficultyText))
            {
                return RedirectToAction(nameof(StartGame));
            }

            float difficulty = ParseDifficulty(difficultyText);

            // Create a fresh board for this user
            _boards[userKey] = new Board(boardSize.Value, difficulty);

            // Reset stored score
            HttpContext.Session.Remove("FinalScore");

            // Clear any one-time "Back to Board" flag so restart behaves normally.
            HttpContext.Session.Remove(AllowFinishedBoardSessionKey);

            return RedirectToAction(nameof(MineSweeperBoard));
        }

        /// <summary>
        /// Displays the Win outcome page with the final score.
        /// </summary>
        [HttpGet]
        public IActionResult Win()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            ViewBag.FinalScore = GetFinalScoreFromSession();
            return View();
        }

        /// <summary>
        /// Displays the Loss outcome page with the final score.
        /// </summary>
        [HttpGet]
        public IActionResult Loss()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            ViewBag.FinalScore = GetFinalScoreFromSession();
            return View();
        }

        /// <summary>
        /// Handles an AJAX left-click for a single cell.
        /// Updates the board state and returns either:
        /// 1) The updated _Cell partial view if the game is still in progress, or
        /// 2) A JSON response instructing the client to redirect to Win or Loss.
        /// </summary>
        /// <param name="id">Flattened cell id (0..Size*Size-1).</param>
        /// <returns>A partial view for a single updated cell, or JSON redirect info if the game ends.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RevealCellAjax(int id)
        {
            // Restrict access: user must be logged in.
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized();
            }

            int userKey = userId.Value;

            // If the user does not have a board initialized yet, stop the request.
            if (!_boards.ContainsKey(userKey))
            {
                return BadRequest();
            }

            Board board = _boards[userKey];

            // Do not allow more clicks after game is finished.
            if (board.DetermineGameState() != Board.GameStatus.InProgress)
            {
                return BadRequest();
            }

            // Convert flattened id to row/column.
            int row = id / board.Size;
            int col = id % board.Size;

            // Validate bounds.
            if (row < 0 || row >= board.Size || col < 0 || col >= board.Size)
            {
                return BadRequest();
            }

            Cell cell = board.Cells[row, col];

            // Reveal logic for left-click.
            if (!cell.IsFlagged && !cell.IsRevealed)
            {
                bool safe = cell.Reveal();

                if (safe && cell.LiveNeighbors == 0)
                {
                    board.FloodFill(row, col);
                }

                if (cell.CollectReward())
                {
                    board.RewardsRemaining++;
                }
            }

            // Save updated board state.
            _boards[userKey] = board;

            // Check whether the move ended the game.
            Board.GameStatus status = board.DetermineGameState();

            if (status != Board.GameStatus.InProgress)
            {
                // Record end time for scoring.
                board.EndTime = DateTime.Now;

                // Calculate and store final score.
                int score = CalculateScore(board);
                SaveFinalScoreToSession(score);

                // Persist finished board state.
                _boards[userKey] = board;

                // Return JSON telling JavaScript where to redirect.
                if (status == Board.GameStatus.Won)
                {
                    return Json(new { redirectUrl = Url.Action("Win", "Game") });
                }

                return Json(new { redirectUrl = Url.Action("Loss", "Game") });
            }

            // Build and return only the updated cell partial.
            CellButtonModel updatedCell = BuildButtonsFromBoard(board)[id];
            return PartialView("_Cell", updatedCell);
        }
    }
}
