using System;
using System.ComponentModel.DataAnnotations;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents a saved Minesweeper game stored in the database.
    /// Each saved game belongs to a specific user and stores the serialized
    /// game board state as JSON text.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Primary key for the saved game record.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The id of the user who owns this saved game.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// The date and time when the game was saved.
        /// </summary>
        [Required]
        public DateTime DateSaved { get; set; }

        /// <summary>
        /// Serialized JSON string containing the saved game state.
        /// This includes the board data and any other values needed to restore the game.
        /// </summary>
        [Required]
        public string GameData { get; set; } = string.Empty;
    }
}