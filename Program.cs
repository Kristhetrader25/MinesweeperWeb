using Microsoft.EntityFrameworkCore;
using MinesweeperWeb.Data;


namespace MinesweeperWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Adds session services to the dependency injection container.
            // This allows the app to store small pieces of user state (like UserId/Username)
            // between HTTP requests (HTTP itself is stateless).
            builder.Services.AddSession();


            /// <summary>
            /// Registers the application's DbContext with the dependency injection container.
            /// Configures SQL Server using the DefaultConnection string from appsettings.json.
            /// </summary>
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                ));

            // This allows the application to maintain user state between HTTP requests.
            builder.Services.AddSession();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enables session middleware for handling session state.
            app.UseSession();


            app.UseAuthorization();

            /// <summary>
            /// Configures default route pattern:
            /// /Controller/Action/Id
            /// </summary>
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

        }
    }
}
