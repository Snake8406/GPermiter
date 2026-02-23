using GPermitter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GPermitter.Services;

public static class IdentitySeeder
{
    private static readonly string[] Roles = ["EndUser", "User", "Admin"];

    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogWarning("Skipping identity seed because the database is not reachable.");
                return;
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminSection = configuration.GetSection("AdminAccount");
            var adminEmail = adminSection["Email"];
            var adminPassword = adminSection["Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    logger.LogWarning("Admin bootstrap account could not be created: {Errors}", string.Join("; ", createResult.Errors.Select(e => e.Description)));
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        catch (SqlException ex)
        {
            logger.LogWarning(ex, "Skipping identity seed because SQL Server is unavailable or database access failed.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex, "Skipping identity seed due to transient SQL failure.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skipping identity seed due to an unexpected startup error.");
        }
    }
}
