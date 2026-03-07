using System.ComponentModel.DataAnnotations;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents the user selections on the StartGame page.
    /// The user chooses a board size and difficulty before starting a new game.
    /// </summary>
    public class StartGameViewModel
    {
        /// <summary>
        /// Selected board size (square grid).
        /// Example: 8 means an 8x8 board.
        /// </summary>
        [Required]
        [Range(4, 30)]
        public int BoardSize { get; set; } = 8;

        /// <summary>
        /// Selected difficulty level.
        /// This will determine mine count or mine percentage.
        /// </summary>
        [Required]
        public string Difficulty { get; set; } = "Easy";
    }
}