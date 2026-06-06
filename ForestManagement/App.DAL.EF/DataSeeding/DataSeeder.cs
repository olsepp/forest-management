using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF.DataSeeding;

public static class DataSeeder
{
    private const string DefaultAdminUsername = "admin";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var dbContext = services.GetRequiredService<AppDbContext>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DataSeeder));

        // Seed roles
        string[] roles = ["Admin"];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new AppRole { Name = roleName });
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {Role}", roleName);
                }
            }
        }

        // Seed admin user from configuration (environment variables in production)
        var adminEmail = config["SeedAdmin:Email"];
        var adminPassword = config["SeedAdmin:Password"];
        var adminUsername = config["SeedAdmin:Username"] ?? DefaultAdminUsername;

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminUsername,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Seeded admin user: {Email}", adminEmail);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to seed admin user: {Errors}", errors);
                }
            }
        }
        else
        {
            logger.LogInformation("SeedAdmin credentials not configured. Skipping admin user creation.");
        }

        // Seed default companies
        var companies = new[]
        {
            new Company
            {
                Name = "Robin Wood Trading OÜ",
                RegistrationNumber = 10000001
            },
            new Company
            {
                Name = "Metsakohin OÜ",
                RegistrationNumber = 10000002
            },
            new Company
            {
                Name = "Windmill OÜ",
                RegistrationNumber = 10000003
            }
        };

        foreach (var company in companies)
        {
            var exists = await dbContext.Companies
                .AnyAsync(c => c.Name == company.Name || c.RegistrationNumber == company.RegistrationNumber);

            if (exists)
            {
                continue;
            }

            await dbContext.Companies.AddAsync(company);
            logger.LogInformation("Seeded company: {Company}", company.Name);
        }

        await dbContext.SaveChangesAsync();
    }
}
