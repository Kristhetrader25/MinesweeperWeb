using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinesweeperWeb.Models
{
    /// <summary>
    /// Represents a registered user within the Minesweeper application.
    /// This entity maps to the Users table in the SQL Server database.
    /// 
    /// Password security is implemented using hashing and salting.
    /// The plain-text password is never stored in the database.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key for the Users table.
        /// Entity Framework Core automatically configures this as an identity column.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's first name.
        /// Required field validated on the server side.
        /// </summary>
        [Required]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name.
        /// Required field validated on the server side.
        /// </summary>
        [Required]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's sex (e.g., Male, Female, Other).
        /// Required field validated on the server side.
        /// </summary>
        [Required]
        public string Sex { get; set; } = string.Empty;

        /// <summary>
        /// User's age.
        /// Must be between 1 and 120.
        /// </summary>
        [Required]
        [Range(1, 120)]
        public int Age { get; set; }

        /// <summary>
        /// Two-letter state abbreviation (e.g., AZ, CA).
        /// Required field validated on the server side.
        /// </summary>
        [Required]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// User's email address.
        /// Must be in valid email format.
        /// </summary>
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Username chosen by the user.
        /// Required field validated on the server side.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password entered from the registration form.
        /// This property is NOT stored in the database.
        /// It is used only for model binding during registration.
        /// </summary>
        [NotMapped]
        [Required]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Securely hashed version of the user's password.
        /// This value is stored in the database instead of the plain-text password.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Random salt used during password hashing.
        /// Stored alongside the password hash in the database.
        /// </summary>
        public string Salt { get; set; } = string.Empty;
    }
}
