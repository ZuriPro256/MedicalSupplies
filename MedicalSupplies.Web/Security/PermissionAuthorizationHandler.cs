using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Security;
using MedicalSupplies.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicalSupplies.Infrastructure.Data;

namespace MedicalSupplies.Web.Security;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public PermissionAuthorizationHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return;

        var userId = _userManager.GetUserId(context.User);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
            return;

        if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            context.Succeed(requirement);
            return;
        }

        var hasPermission = await _db.UserPermissions
            .AsNoTracking()
            .AnyAsync(p =>
                p.UserId == userId &&
                p.Permission == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);
    }
}
