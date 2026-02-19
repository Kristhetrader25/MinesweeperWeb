using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MinesweeperWeb.Data;
using MinesweeperWeb.Models;
using MinesweeperWeb.Security;



namespace MinesweeperWeb.Controllers
{
    /// <summary>
    /// Handles user-related pages and actions (Register, Login, Logout).
    /// Supports:
    /// - Register (GET/POST)
    /// - Login (GET/POST)
    /// - Session-based access checks for restricted pages
    /// </summary>
    public class UserController : Controller
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Creates a new instance of the UserController.
        /// AppDbContext is provided by dependency injection (configured in Program.cs).
        /// </summary>
        /// <param name="db">Database context used to read/write user data.</param>
        public UserController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Displays the registration form (GET).
        /// Route: /User/Register
        /// </summary>
        /// <returns>The Register view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Processes the registration form submission (POST).
        /// - Uses HTTP POST to controller
        /// - Performs server-side validation (ModelState)
        /// - Stores user information in SQL Server
        /// - Password is stored securely using hashing + salting (NOT plain text)
        /// - Forwards to success or error page
        /// </summary>
        /// <param name="user">User model bound from the posted form fields.</param>
        /// <returns>Redirects to Success on valid save; otherwise shows Error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            // Server-side validation based on DataAnnotations in the User model.
            if (!ModelState.IsValid)
            {
                // Return the same view so validation messages can display.
                return View(user);
            }

            try
            {
                // 1) Generate a unique random salt for this user.
                user.Salt = PasswordHelper.GenerateSalt();

                // 2) Hash the plain-text password using the salt.
                user.PasswordHash = PasswordHelper.HashPassword(user.Password, user.Salt);

                // 3) Clear the plain-text password so it is not accidentally used later.
                //    Note: Password is [NotMapped], so it will not be saved to the DB anyway.
                user.Password = string.Empty;

                // 4) Save the user record (with PasswordHash and Salt) to the database.
                _db.Users.Add(user);
                _db.SaveChanges();

                return RedirectToAction("RegisterSuccess");
            }
            catch
            {
                return RedirectToAction("RegisterError");
            }
        }


        /// <summary>
        /// Shows a simple success message after registration completes.
        /// </summary>
        /// <returns>The RegisterSuccess view.</returns>
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        /// <summary>
        /// Shows a simple error message if registration fails.
        /// </summary>
        /// <returns>The RegisterError view.</returns>
        [HttpGet]
        public IActionResult RegisterError()
        {
            return View();
        }


        /// <summary>
        /// Displays the login form (GET).
        /// Route: /User/Login
        /// </summary>
        /// <returns>The Login view.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Processes the login form submission (POST).
        /// This method:
        /// - Locates the user in SQL Server by username.
        /// - Verifies the password using the stored hash + salt.
        /// - If valid, creates session values to track authentication.
        /// </summary>
        /// <param name="model">Login form input (username and password).</param>
        /// <returns>
        /// Redirects to Game/StartGame if successful; otherwise redirects to LoginError.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            // Server-side validation of login fields.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Look up the user by username.
            User? user = _db.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                // Username not found.
                return RedirectToAction("LoginError");
            }

            // Verify the password attempt using PBKDF2 hash + salt.
            bool passwordValid = PasswordHelper.VerifyPassword(
                model.Password,
                user.PasswordHash,
                user.Salt);

            if (!passwordValid)
            {
                // Password incorrect.
                return RedirectToAction("LoginError");
            }

            // Store minimal user identity info in session.
            // This is how app remembers the logged-in user across requests.
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            // For creating GameController/StartGame.
            return RedirectToAction("StartGame", "Game");
        }

        /// <summary>
        /// Displays a login error page when authentication fails.
        /// </summary>
        /// <returns>The LoginError view.</returns>
        [HttpGet]
        public IActionResult LoginError()
        {
            return View();
        }

        /// <summary>
        /// Logs the user out by clearing all session values.
        /// This ends the authenticated session and returns the user to a logout confirmation page.
        /// </summary>
        /// <returns>The Logout view.</returns>
        [HttpGet]
        public IActionResult Logout()
        {
            // Clears all session values (UserId, Username, etc.).
            HttpContext.Session.Clear();

            return View();
        }


    }
}

