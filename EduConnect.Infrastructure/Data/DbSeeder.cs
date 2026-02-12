using EduConnect.Domain.Entities;
using EduConnect.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduConnect.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        await context.Database.EnsureCreatedAsync();

        var rolesSection = configuration.GetSection("SeedData:Roles");
        var roles = rolesSection.Get<string[]>();
        if (roles == null || roles.Length == 0)
            throw new InvalidOperationException("SeedData:Roles is required in config (e.g. [\"Admin\", \"Teacher\", \"Parent\"]).");
        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role)) continue;
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var seedAdmin = configuration.GetSection("SeedData:DefaultAdmin");
        var adminEmail = seedAdmin["Email"] ?? throw new InvalidOperationException("SeedData:DefaultAdmin:Email is required.");
        var adminPassword = seedAdmin["Password"] ?? throw new InvalidOperationException("SeedData:DefaultAdmin:Password is required.");
        var adminFullName = seedAdmin["FullName"] ?? throw new InvalidOperationException("SeedData:DefaultAdmin:FullName is required.");
        var adminPhone = seedAdmin["PhoneNumber"] ?? throw new InvalidOperationException("SeedData:DefaultAdmin:PhoneNumber is required.");
        var defaultAdminRole = seedAdmin["Role"]?.Trim();
        if (string.IsNullOrEmpty(defaultAdminRole))
            defaultAdminRole = roles[0];

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                PhoneNumber = adminPhone,
                Role = UserRole.Admin,
                MustChangePassword = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, defaultAdminRole);
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
