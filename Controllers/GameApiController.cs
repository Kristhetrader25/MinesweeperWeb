using Microsoft.AspNetCore.Mvc;
using MinesweeperWeb.Data;
using MinesweeperWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MinesweeperWeb.Controllers
{
    /// <summary>
    /// Provides REST API endpoints for viewing and deleting saved Minesweeper games.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class GameApiController : ControllerBase
    {
        /// <summary>
        /// Database context used to access saved game records.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Creates a new GameApiController with database access.
        /// </summary>
        /// <param name="context">Application database context.</param>
        public GameApiController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays all saved games in the database.
        /// </summary>
        /// <returns>A list of all saved game records.</returns>
        [HttpGet("showSavedGames")]
        public ActionResult<List<Game>> ShowSavedGames()
        {
            List<Game> games = _context.Games
                .OrderByDescending(g => g.DateSaved)
                .ToList();

            return Ok(games);
        }

        /// <summary>
        /// Displays the contents of one saved game specified by id.
        /// </summary>
        /// <param name="id">The id of the saved game to retrieve.</param>
        /// <returns>The matching saved game record, or NotFound if missing.</returns>
        [HttpGet("showSavedGames/{id}")]
        public ActionResult<Game> ShowSavedGameById(int id)
        {
            Game? game = _context.Games.FirstOrDefault(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return Ok(game);
        }

        /// <summary>
        /// Deletes one saved game from the database by id.
        /// </summary>
        /// <param name="id">The id of the saved game to delete.</param>
        /// <returns>A success message if deleted, or NotFound if missing.</returns>
        [HttpDelete("deleteOneGame/{id}")]
        public IActionResult DeleteSavedGame(int id)
        {
            Game? game = _context.Games.FirstOrDefault(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            _context.SaveChanges();

            return Ok(new { message = $"Game {id} deleted successfully." });
        }
    }
}