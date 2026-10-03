using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.Services;
using MedicalSupplies.Web.ViewModels.Admin.Inquiries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,Sales")]
public class InquiriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICustomerNotificationService _customerNotificationService;

    public InquiriesController(
        ApplicationDbContext context,
        ICustomerNotificationService customerNotificationService)
    {
        _context = context;
        _customerNotificationService = customerNotificationService;
    }

    public async Task<IActionResult> Index(EnquiryStatus? status)
    {
        var query = _context.Enquiries
            .AsNoTracking()
            .Include(e => e.Customer)
            .AsQueryable();

        var newCount = await _context.Enquiries
            .CountAsync(e => e.Status == EnquiryStatus.New);

        var inProgressCount = await _context.Enquiries
            .CountAsync(e => e.Status == EnquiryStatus.InProgress);

        var resolvedCount = await _context.Enquiries
            .CountAsync(e => e.Status == EnquiryStatus.Resolved);

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        var inquiries = await query
            .OrderByDescending(e => e.CreatedDate)
            .Select(e => new InquiryListItemViewModel
            {
                EnquiryId = e.EnquiryId,
                CustomerId = e.CustomerId,
                OrganizationName = e.Customer != null
                    ? e.Customer.OrganizationName
                    : null,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                Subject = e.Subject,
                MessagePreview = e.Message.Length > 100
                    ? e.Message.Substring(0, 100) + "..."
                    : e.Message,
                Status = e.Status,
                CreatedDate = e.CreatedDate
            })
            .ToListAsync();

        ViewBag.SelectedStatus = status;
        ViewBag.NewCount = newCount;
        ViewBag.InProgressCount = inProgressCount;
        ViewBag.ResolvedCount = resolvedCount;
        ViewBag.TotalCount = newCount + inProgressCount + resolvedCount;

        return View(inquiries);
    }

    public async Task<IActionResult> Details(int id)
    {
        var enquiry = await _context.Enquiries
            .AsNoTracking()
            .Include(e => e.Customer)
            .FirstOrDefaultAsync(e => e.EnquiryId == id);

        if (enquiry is null)
            return NotFound();

        var vm = new InquiryDetailsViewModel
        {
            EnquiryId = enquiry.EnquiryId,
            CustomerId = enquiry.CustomerId,
            OrganizationName = enquiry.Customer?.OrganizationName,
            Name = enquiry.Name,
            Email = enquiry.Email,
            Phone = enquiry.Phone,
            Subject = enquiry.Subject,
            Message = enquiry.Message,
            Status = enquiry.Status,
            CreatedDate = enquiry.CreatedDate
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        EnquiryStatus status)
    {
        if (!Enum.IsDefined(typeof(EnquiryStatus), status))
        {
            TempData["Error"] = "Invalid inquiry status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var enquiry = await _context.Enquiries
            .FirstOrDefaultAsync(e => e.EnquiryId == id);

        if (enquiry is null)
            return NotFound();

        var previousStatus = enquiry.Status;

        enquiry.Status = status;

        await _context.SaveChangesAsync();

        if (status == EnquiryStatus.Resolved &&
            previousStatus != EnquiryStatus.Resolved &&
            enquiry.CustomerId.HasValue)
        {
            var customerUserId = await _context.Customers
                .Where(c => c.CustomerId == enquiry.CustomerId.Value)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(customerUserId))
            {
                await _customerNotificationService.CreateAsync(
                    customerUserId,
                    "InquiryResolved",
                    "Enquiry update",
                    "Your enquiry has been reviewed and resolved. Please check your Business Account for the update.",
                    "/Account/Updates");
            }
        }

        TempData["Success"] = "Inquiry status updated successfully.";

        return RedirectToAction(nameof(Details), new { id });
    }
}
