using MedicalSupplies.Core.Security;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.Security;
using MedicalSupplies.Web.ViewModels.Admin.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class StaffController : Controller
{
    private static readonly string[] StaffRoles =
    {
        "SuperAdmin",
        "Admin",
        "Sales",
        "InventoryManager"
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly PermissionService _permissionService;

    public StaffController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        PermissionService permissionService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
    }

    [Authorize(Policy = Permissions.Staff.View)]
    public async Task<IActionResult> Index(string? search)
    {
        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            users = users.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        var staff = new List<StaffListItemViewModel>();

        foreach (var user in users.OrderBy(u => u.FullName).ToList())
        {
            var roles = await _userManager.GetRolesAsync(user);
            var staffRole = roles.FirstOrDefault(r => StaffRoles.Contains(r));

            if (staffRole is null)
                continue;

            staff.Add(new StaffListItemViewModel
            {
                UserId = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                Role = staffRole,
                IsActive = user.IsActive,
                IsLockedOut = await _userManager.IsLockedOutAsync(user),
                EmailConfirmed = user.EmailConfirmed
            });
        }

        ViewBag.Search = search;
        return View(staff);
    }

    [Authorize(Policy = Permissions.Staff.View)]
    [Authorize(Policy = Permissions.Staff.Create)]
    public IActionResult Create()
    {
        var defaults = RolePermissions.ForRole("Sales")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var model = new StaffCreateViewModel
        {
            Role = "Sales",
            IsActive = true,
            Permissions = defaults.ToList(),
            AvailablePermissions = BuildPermissionOptions(defaults)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Staff.Create)]
    public async Task<IActionResult> Create(StaffCreateViewModel vm)
    {
        if (!StaffRoles.Contains(vm.Role))
        {
            ModelState.AddModelError(nameof(vm.Role), "Invalid staff role.");
        }

        var canManagePermissions = await CurrentUserCanManagePermissionsAsync();

        if (!canManagePermissions)
        {
            vm.Permissions = RolePermissions.ForRole(vm.Role).ToList();
        }
        else
        {
            vm.Permissions = _permissionService
                .FilterValidPermissions(vm.Permissions)
                .ToList();
        }

        if (!ModelState.IsValid)
        {
            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        var existingUser = await _userManager.FindByEmailAsync(vm.Email.Trim());

        if (existingUser is not null)
        {
            ModelState.AddModelError(
                nameof(vm.Email),
                "That email address is already registered.");

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email.Trim(),
            Email = vm.Email.Trim(),
            FullName = vm.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(vm.Phone)
                ? null
                : vm.Phone.Trim(),
            EmailConfirmed = true,
            IsActive = vm.IsActive
        };

        var createResult = await _userManager.CreateAsync(user, vm.Password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, vm.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        await _permissionService.SetUserPermissionsAsync(
            user.Id,
            vm.Permissions);

        if (!vm.IsActive)
        {
            await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100));
        }

        TempData["Success"] = "Staff account created successfully.";
        return RedirectToAction(nameof(Details), new { id = user.Id });
    }

    [Authorize(Policy = Permissions.Staff.View)]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Any(r => StaffRoles.Contains(r)))
            return NotFound();

        var model = new StaffListItemViewModel
        {
            UserId = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            Role = roles.FirstOrDefault(r => StaffRoles.Contains(r)) ?? string.Empty,
            IsActive = user.IsActive,
            IsLockedOut = await _userManager.IsLockedOutAsync(user),
            EmailConfirmed = user.EmailConfirmed
        };

        return View(model);
    }

    [Authorize(Policy = Permissions.Staff.Edit)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault(r => StaffRoles.Contains(r));

        if (currentRole is null)
            return NotFound();

        var userPermissions = await _permissionService.GetUserPermissionsAsync(user.Id);

        return View(new StaffEditViewModel
        {
            UserId = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            Role = currentRole,
            IsActive = user.IsActive,
            Permissions = userPermissions.ToList(),
            AvailablePermissions = BuildPermissionOptions(userPermissions)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Staff.Edit)]
    public async Task<IActionResult> Edit(StaffEditViewModel vm)
    {
        var user = await _userManager.FindByIdAsync(vm.UserId ?? string.Empty);

        if (user is null)
            return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault(r => StaffRoles.Contains(r));

        if (currentRole is null)
            return NotFound();

        var currentPermissions =
            await _permissionService.GetUserPermissionsAsync(user.Id);

        var canManagePermissions =
            await CurrentUserCanManagePermissionsAsync();

        if (!canManagePermissions)
        {
            vm.Permissions = currentPermissions.ToList();
        }
        else
        {
            vm.Permissions = _permissionService
                .FilterValidPermissions(vm.Permissions)
                .ToList();
        }

        if (!ModelState.IsValid)
        {
            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        if (!StaffRoles.Contains(vm.Role))
        {
            ModelState.AddModelError(nameof(vm.Role), "Invalid staff role.");
            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        var currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId &&
            currentRole == "SuperAdmin" &&
            vm.Role != "SuperAdmin")
        {
            ModelState.AddModelError(
                nameof(vm.Role),
                "You cannot remove your own SuperAdmin role.");

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        if (user.Id == currentUserId &&
            currentRole == "SuperAdmin" &&
            !vm.IsActive)
        {
            ModelState.AddModelError(
                nameof(vm.IsActive),
                "You cannot deactivate your own SuperAdmin account.");

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        if (user.Id == currentUserId &&
            currentRole == "SuperAdmin" &&
            canManagePermissions &&
            !vm.Permissions.Contains(
                Permissions.Staff.ManagePermissions,
                StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                string.Empty,
                "You cannot remove your own staff-management permission.");

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        user.FullName = vm.FullName.Trim();

        user.PhoneNumber = string.IsNullOrWhiteSpace(vm.Phone)
            ? null
            : vm.Phone.Trim();

        if (!string.Equals(
                user.Email,
                vm.Email.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            var existingUser =
                await _userManager.FindByEmailAsync(vm.Email.Trim());

            if (existingUser is not null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    nameof(vm.Email),
                    "That email address is already in use.");

                vm.AvailablePermissions =
                    BuildPermissionOptions(vm.Permissions);

                return View(vm);
            }

            user.Email = vm.Email.Trim();
            user.UserName = vm.Email.Trim();
            user.NormalizedEmail =
                _userManager.NormalizeEmail(user.Email);
            user.NormalizedUserName =
                _userManager.NormalizeName(user.UserName);
        }

        user.IsActive = vm.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            vm.AvailablePermissions = BuildPermissionOptions(vm.Permissions);
            return View(vm);
        }

        var rolesAfterUpdate = await _userManager.GetRolesAsync(user);

        foreach (var role in rolesAfterUpdate.Where(r => StaffRoles.Contains(r)))
        {
            if (role == vm.Role)
                continue;

            var removeResult = await _userManager.RemoveFromRoleAsync(user, role);

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                vm.AvailablePermissions =
                    BuildPermissionOptions(vm.Permissions);

                return View(vm);
            }
        }

        var rolesAfterRemoval = await _userManager.GetRolesAsync(user);

        if (!rolesAfterRemoval.Contains(vm.Role))
        {
            var addResult = await _userManager.AddToRoleAsync(user, vm.Role);

            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                vm.AvailablePermissions =
                    BuildPermissionOptions(vm.Permissions);

                return View(vm);
            }
        }

        if (vm.IsActive)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100));
        }

        if (canManagePermissions)
        {
            await _permissionService.SetUserPermissionsAsync(
                user.Id,
                vm.Permissions);
        }

        TempData["Success"] = "Staff account updated successfully.";
        return RedirectToAction(nameof(Details), new { id = user.Id });
    }

    private static List<PermissionOptionViewModel> BuildPermissionOptions(
        IEnumerable<string> selectedPermissions)
    {
        var selected = selectedPermissions
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return PermissionCatalog.All
            .Select(p => new PermissionOptionViewModel
            {
                Category = p.Category,
                Description = p.Description,
                Permission = p.Permission,
                Selected = selected.Contains(p.Permission)
            })
            .ToList();
    }

    private async Task<bool> CurrentUserCanManagePermissionsAsync()
    {
        var currentUserId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return false;

        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (currentUser is null || !currentUser.IsActive)
            return false;

        if (await _userManager.IsInRoleAsync(currentUser, "SuperAdmin"))
            return true;

        var permissions =
            await _permissionService.GetUserPermissionsAsync(currentUser.Id);

        return permissions.Contains(
            Permissions.Staff.ManagePermissions);
    }
}
