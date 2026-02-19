using Microsoft.EntityFrameworkCore;
using MinesweeperWeb.Models;

namespace MinesweeperWeb.Data
{
    /// <summary>
    /// Represents the application's database context.
    /// This class manages entity objects during runtime,
    /// including retrieving, saving, and updating data.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Constructor receives DbContextOptions from dependency injection.
        /// </summary>
        /// <param name="options">Configuration options for the DbContext.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Represents the Users table in the database.
        /// </summary>
        public DbSet<User> Users { get; set; }
    }
}

