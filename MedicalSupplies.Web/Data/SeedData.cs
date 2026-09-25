using Microsoft.AspNetCore.Identity;

namespace MedicalSupplies.Web.Data;

/// <summary>
/// Creates the fixed set of application roles on startup. Seeding an
/// actual SuperAdmin *user* is deliberately left out — create that one
/// through a setup script or the Identity UI the first time you deploy,
/// rather than hard-coding a default password here.
/// </summary>
public static class SeedData
{
    private static readonly string[] Roles =
    {
        "SuperAdmin",
        "Admin",
        "Sales",
        "InventoryManager",
        "Customer"
    };

    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
