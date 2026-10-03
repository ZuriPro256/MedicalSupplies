using System.Security.Claims;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Core.Security;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Services;

public interface IAdminNotificationService
{
    Task<AdminNotificationCountsViewModel> GetCountsAsync(ClaimsPrincipal user);
}

public class AdminNotificationService : IAdminNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    // Reuse the same result during one HTTP request.
    private AdminNotificationCountsViewModel? _cachedCounts;

    public AdminNotificationService(
        ApplicationDbContext context,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<AdminNotificationCountsViewModel> GetCountsAsync(
        ClaimsPrincipal user)
    {
        if (_cachedCounts is not null)
            return _cachedCounts;

        var model = new AdminNotificationCountsViewModel
        {
            CanSeeInquiries =
                user.IsInRole("SuperAdmin") ||
                user.IsInRole("Admin") ||
                user.IsInRole("Sales"),

            CanSeeQuotations =
                user.IsInRole("SuperAdmin") ||
                user.IsInRole("Admin") ||
                user.IsInRole("Sales"),

            CanSeeOrders =
                user.IsInRole("SuperAdmin") ||
                user.IsInRole("Admin") ||
                user.IsInRole("Sales"),

            CanSeePurchaseOrders =
                user.IsInRole("SuperAdmin") ||
                user.IsInRole("Admin") ||
                user.IsInRole("InventoryManager"),

            CanSeeAccountRecovery =
                (await _authorizationService.AuthorizeAsync(
                    user,
                    Permissions.AccountRecovery.View)).Succeeded
        };

        // "Needs attention now" counts only.
        if (model.CanSeeInquiries)
        {
            model.Inquiries = await _context.Enquiries
                .CountAsync(e => e.Status == EnquiryStatus.New);
        }

        if (model.CanSeeQuotations)
        {
            // A new quotation request waiting for staff action.
            model.Quotations = await _context.Quotations
                .CountAsync(q => q.Status == QuotationStatus.Pending);
        }

        if (model.CanSeeOrders)
        {
            // Customer orders that have not yet been moved into Processing.
            model.Orders = await _context.Orders
                .CountAsync(o => o.OrderStatus == OrderStatus.Created);
        }

        if (model.CanSeePurchaseOrders)
        {
            // Staff-created Drafts do not notify anybody.
            // Submitted POs are waiting for approval.
            model.PurchaseOrders = await _context.PurchaseOrders
                .CountAsync(po => po.Status == PurchaseOrderStatus.Submitted);
        }

        if (model.CanSeeAccountRecovery)
        {
            model.AccountRecovery = await _context.AccountRecoveryRequests
                .CountAsync(r => r.Status == "Pending");
        }

        _cachedCounts = model;
        return model;
    }
}
