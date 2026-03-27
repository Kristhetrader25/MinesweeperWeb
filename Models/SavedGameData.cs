using System;
using System.Collections.Generic;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents the serialized game state that will be stored
    /// in the database as JSON.
    /// </summary>
    public class SavedGameData
    {
        /// <summary>
        /// The size of the board.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// The difficulty value used to generate bombs.
        /// </summary>
        public float Difficulty { get; set; }

        /// <summary>
        /// The number of rewards currently available to the player.
        /// </summary>
        public int RewardsRemaining { get; set; }

        /// <summary>
        /// The time the game originally started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The time the game ended, if applicable.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// A flattened list of saved cell states used to rebuild the board.
        /// </summary>
        public List<SavedCellData> Cells { get; set; } = new List<SavedCellData>();
    }
}