namespace MinesweeperWeb.Models
{
    /// <summary>
    /// UI-friendly model for rendering a Minesweeper cell as a clickable button.
    /// </summary>
    public class CellButtonModel
    {
        /// <summary>
        /// Unique id used for form postback (0..(Size*Size-1)).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Row index in the board grid.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Column index in the board grid.
        /// </summary>
        public int Col { get; set; }

        /// <summary>
        /// Text to display in the button (for testing).
        /// Examples: ".", "F", "1", " ", "B"
        /// </summary>
        public string DisplayText { get; set; } = ".";

        /// <summary>
        /// Image filename for the cell.
        /// </summary>
        public string ImageName { get; set; } = "Tile 1.png";

        /// <summary>
        /// Alt text for the cell.
        /// </summary>
        public string AltText { get; set; } = "Hidden cell";
    }
}