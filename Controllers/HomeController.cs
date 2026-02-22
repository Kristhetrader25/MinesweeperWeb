using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Models;

namespace MinesweeperWeb.Controllers
{
    /// <summary>
    /// Handles general site navigation pages such as Home, Privacy, and Error.
    /// This controller does not require authentication and serves as the
    /// default landing controller for the application.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor for HomeController.
        /// An ILogger instance is injected via dependency injection
        /// to allow logging of informational messages, warnings, or errors.
        /// </summary>
        /// <param name="logger">Logger used for application diagnostics.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays the default home page of the application.
        /// Route: /Home/Index (or simply / by default routing).
        /// </summary>
        /// <returns>The Index view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the Privacy page.
        /// This page typically contains general informational content.
        /// </summary>
        /// <returns>The Privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page when an unhandled exception occurs.
        /// Response caching is disabled to prevent error responses from being stored.
        /// The ErrorViewModel includes a RequestId to help trace and debug issues.
        /// </summary>
        /// <returns>The Error view populated with diagnostic information.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
