using MedicalSupplies.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,Sales,InventoryManager")]
public class DashboardController : Controller
{
    private readonly IAdminNotificationService _notificationService;

    public DashboardController(IAdminNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var notificationCounts =
            await _notificationService.GetCountsAsync(User);

        return View(notificationCounts);
    }
}
