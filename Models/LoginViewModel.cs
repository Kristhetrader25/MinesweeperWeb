using System.ComponentModel.DataAnnotations;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents the input fields required for user login.
    /// This model is used strictly for form binding and validation.
    /// It is NOT mapped to a database table.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Username entered by the user.
        /// Required for authentication.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password entered by the user.
        /// Used only for verification against stored hash.
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
