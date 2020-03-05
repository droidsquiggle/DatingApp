using System;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // we now break out the .run from the host buildier beacuse we want to seed our DB before we run the application
            // we need to get the db context so we need to create a scoped version 
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // create database
                    var context = services.GetRequiredService<DataContext>();
                    // create userManager context
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    // create RoleManagercontext
                    var roleManager = services.GetRequiredService<RoleManager<Role>>();
                    // run database migrations
                    context.Database.Migrate();
                    // seed users from database
                    Seed.SeedUsers(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occured during migration");
                }
            }
            // after migration and seeding of the database, then run the host
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
