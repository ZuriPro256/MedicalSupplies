using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.ViewComponents;

public class LoggedInAsViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public LoggedInAsViewComponent(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return Content(string.Empty);

        var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

        if (user == null)
            return Content(string.Empty);

        var roles = await _userManager.GetRolesAsync(user);

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        string accountName;
        string accountType;

        if (roles.Contains("Customer", StringComparer.OrdinalIgnoreCase))
        {
            accountName =
                !string.IsNullOrWhiteSpace(customer?.OrganizationName)
                    ? customer.OrganizationName.Trim()
                    : $"{customer?.FirstName} {customer?.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(accountName))
                accountName = user.FullName?.Trim()
                    ?? user.Email
                    ?? "Customer";

            accountType = "Business Account";
        }
        else
        {
            accountName = "ZURIPRO MEDICAL SUPPLIES";

            var staffRole = GetStaffRole(roles);

            accountType = $"{staffRole} Account";
        }

        return View(new LoggedInAsViewModel
        {
            AccountName = accountName,
            Role = accountType
        });
    }

    private static string GetStaffRole(IList<string> roles)
    {
        string[] staffRoles =
        [
            "SuperAdmin",
            "Admin",
            "Sales",
            "InventoryManager"
        ];

        return staffRoles.FirstOrDefault(
                   role => roles.Contains(
                       role,
                       StringComparer.OrdinalIgnoreCase))
               ?? "Staff";
    }
}

public sealed class LoggedInAsViewModel
{
    public string AccountName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
