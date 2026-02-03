using EduConnect.Domain.Entities;
using EduConnect.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduConnect.Infrastructure.Data;

public static class DbSeeder
{
    private static readonly string[] DefaultRoles = { "Admin", "Teacher", "Parent" };

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        await context.Database.EnsureCreatedAsync();

        var roles = configuration.GetSection("SeedData:Roles").Get<string[]>() ?? DefaultRoles;
        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role)) continue;
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var seedAdmin = configuration.GetSection("SeedData:DefaultAdmin");
        var adminEmail = seedAdmin["Email"] ?? "admin@educonnect.com";
        var adminPassword = seedAdmin["Password"] ?? "Admin@123";
        var adminFirstName = seedAdmin["FirstName"] ?? "Admin";
        var adminLastName = seedAdmin["LastName"] ?? "User";
        var adminPhone = seedAdmin["PhoneNumber"] ?? "+959123456789";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
                PhoneNumber = adminPhone,
                Role = UserRole.Admin,
                MustChangePassword = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Default admin account created (from config SeedData:DefaultAdmin):");
                Console.WriteLine($"Email: {adminEmail}");
                Console.WriteLine("Please change the password after first login!");
            }
            else
            {
                Console.WriteLine("Failed to create admin account:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"- {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("Admin account already exists.");
        }
    }
}
