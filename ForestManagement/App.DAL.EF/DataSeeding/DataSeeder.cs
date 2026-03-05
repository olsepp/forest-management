using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF.DataSeeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var dbContext = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DataSeeder));

        // Seed roles
        string[] roles = ["Admin", "Employee"];
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

        // Seed default admin user
        const string adminEmail = "admin@forestmanagement.ee";
        const string adminUsername = "admin";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = adminUsername,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
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

        // Seed default employee users
        var employeeUsers = new[]
        {
            new
            {
                Email = "employee1@forestmanagement.ee",
                UserName = "employee1",
                FirstName = "Regular",
                LastName = "EmployeeOne",
                Password = "Employee123!"
            },
            new
            {
                Email = "employee2@forestmanagement.ee",
                UserName = "employee2",
                FirstName = "Regular",
                LastName = "EmployeeTwo",
                Password = "Employee123!"
            }
        };

        foreach (var employee in employeeUsers)
        {
            var existingEmployee = await userManager.FindByEmailAsync(employee.Email);
            if (existingEmployee != null)
            {
                continue;
            }

            var employeeUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = employee.UserName,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(employeeUser, employee.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
                logger.LogInformation("Seeded employee user: {Email}", employee.Email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to seed employee user {Email}: {Errors}", employee.Email, errors);
            }
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
