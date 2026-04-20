using BlindMatchPAS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BlindMatchPAS.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    /// <summary>
    /// Creates or promotes the dev admin from <c>Seed:AdminEmail</c> / <c>Seed:AdminPassword</c> (Development only).
    /// </summary>
    public static async Task SeedDevelopmentAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return;

        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
                await userManager.AddToRoleAsync(user, RoleNames.Admin);
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Administrator"
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Dev admin seed failed: {msg}");
        }

        await userManager.AddToRoleAsync(user, RoleNames.Admin);
    }

    private static readonly string[] AllRoles =
    [
        RoleNames.Student,
        RoleNames.Supervisor,
        RoleNames.Admin
    ];
}
