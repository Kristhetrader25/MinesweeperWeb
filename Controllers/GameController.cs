using Microsoft.AspNetCore.Mvc;

namespace MinesweeperWeb.Controllers
{
    /// <summary>
    /// Handles game-related pages for the Minesweeper application.
    /// For this Milestone, StartGame is restricted to authenticated users using session checks.
    /// </summary>
    public class GameController : Controller
    {
        /// <summary>
        /// Displays the StartGame page only if the user is logged in.
        /// A user is considered logged in when the session contains a valid UserId.
        /// </summary>
        /// <returns>
        /// Redirects to User/Login if no session exists; otherwise returns the StartGame view.
        /// </returns>
        [HttpGet]
        public IActionResult StartGame()
        {
            // If there is no UserId in session, the user is not authenticated.
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // Redirect unauthenticated users to the login page.
                return RedirectToAction("Login", "User");
            }

            // User is authenticated, allow access to StartGame.
            return View();
        }
    }
}

