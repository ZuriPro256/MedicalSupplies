using MedicalSupplies.Core.Security;
using MedicalSupplies.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Security;

public sealed class PermissionService
{
    private readonly ApplicationDbContext _db;

    public PermissionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(string userId)
    {
        var permissions = await _db.UserPermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Permission)
            .ToListAsync();

        return permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task SetUserPermissionsAsync(
        string userId,
        IEnumerable<string> permissions)
    {
        var requested = FilterValidPermissions(permissions);

        var existing = await _db.UserPermissions
            .Where(p => p.UserId == userId)
            .ToListAsync();

        _db.UserPermissions.RemoveRange(existing);

        foreach (var permission in requested)
        {
            _db.UserPermissions.Add(new Core.Entities.UserPermission
            {
                UserId = userId,
                Permission = permission
            });
        }

        await _db.SaveChangesAsync();
    }

    public IReadOnlyCollection<string> FilterValidPermissions(
        IEnumerable<string> permissions)
    {
        var validPermissions = PermissionCatalog.All
            .Select(p => p.Permission)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return permissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Where(p => validPermissions.Contains(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task SetRoleDefaultsAsync(string userId, string role)
    {
        var defaults = RolePermissions.ForRole(role);
        await SetUserPermissionsAsync(userId, defaults);
    }
}
