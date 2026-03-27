namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents the serialized state of a single cell on the board.
    /// </summary>
    public class SavedCellData
    {
        /// <summary>
        /// Indicates whether the cell contains a bomb.
        /// </summary>
        public bool Live { get; set; }

        /// <summary>
        /// The number of neighboring bomb cells.
        /// </summary>
        public int LiveNeighbors { get; set; }

        /// <summary>
        /// Indicates whether the cell contains a reward.
        /// </summary>
        public bool HasReward { get; set; }

        /// <summary>
        /// Indicates whether the cell has been revealed.
        /// </summary>
        public bool IsRevealed { get; set; }

        /// <summary>
        /// Indicates whether the cell is flagged.
        /// </summary>
        public bool IsFlagged { get; set; }
    }
}
