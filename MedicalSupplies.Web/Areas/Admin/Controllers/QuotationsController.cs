using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,Sales")]
public class QuotationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuotationsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(QuotationStatus? status)
    {
        var query = _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Details)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        var quotations = await query
            .OrderByDescending(q => q.RequestDate)
            .Select(q => new QuotationListItemViewModel
            {
                QuotationId = q.QuotationId,
                QuotationNumber = q.QuotationNumber,
                CustomerName = q.Customer.OrganizationName ?? (q.Customer.FirstName + " " + q.Customer.LastName),
                RequestDate = q.RequestDate,
                LineCount = q.Details.Count,
                TotalAmount = q.TotalAmount,
                Status = q.Status
            })
            .ToListAsync();

        ViewBag.SelectedStatus = status;
        return View(quotations);
    }

    public async Task<IActionResult> Details(int id)
    {
        var quotation = await LoadQuotationAsync(id);
        if (quotation is null) return NotFound();

        return View(ToViewModel(quotation));
    }

    /// <summary>
    /// Handles both "Save Quote" and "Send to Customer" — which one happened
    /// is carried in the submitAction field set by the two submit buttons.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePricing(QuotationPricingViewModel vm, string submitAction)
    {
        var quotation = await LoadQuotationAsync(vm.QuotationId);
        if (quotation is null) return NotFound();

        if (quotation.Status is QuotationStatus.Accepted or QuotationStatus.Rejected
            or QuotationStatus.Expired or QuotationStatus.ConvertedToOrder)
        {
            TempData["Error"] = "This quotation is no longer editable.";
            return RedirectToAction(nameof(Details), new { id = vm.QuotationId });
        }

        if (!ModelState.IsValid)
        {
            var reloaded = ToViewModel(quotation);
            reloaded.DeliveryLocation = vm.DeliveryLocation;
            reloaded.DiscountAmount = vm.DiscountAmount;
            reloaded.ValidUntil = vm.ValidUntil;
            reloaded.AdminNotes = vm.AdminNotes;
            return View(nameof(Details), reloaded);
        }

        foreach (var line in vm.Lines)
        {
            var detail = quotation.Details.First(d => d.QuotationDetailId == line.QuotationDetailId);
            detail.UnitPrice = line.UnitPrice;
            detail.TotalPrice = (line.UnitPrice ?? 0) * detail.Quantity;
        }

        quotation.DeliveryLocation = vm.DeliveryLocation;
        quotation.DiscountAmount = vm.DiscountAmount;
        quotation.ValidUntil = vm.ValidUntil;
        quotation.AdminNotes = vm.AdminNotes;

        var subTotal = quotation.Details.Sum(d => d.TotalPrice ?? 0);
        quotation.TotalAmount = Math.Max(0, subTotal - (vm.DiscountAmount ?? 0));

        if (submitAction == "send")
        {
            quotation.Status = QuotationStatus.Sent;
            quotation.PreparedDate = DateTime.UtcNow;
            quotation.PreparedBy = User.Identity?.Name;
        }
        else
        {
            quotation.Status = QuotationStatus.Pricing;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = submitAction == "send" ? "Quotation marked as sent." : "Quotation saved.";
        return RedirectToAction(nameof(Details), new { id = vm.QuotationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var quotation = await _context.Quotations.FindAsync(id);
        if (quotation is null) return NotFound();

        // A quotation only reaches Accepted after the customer has confirmed —
        // by phone/email/WhatsApp — that they want to go ahead with the priced quote.
        quotation.Status = QuotationStatus.Accepted;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Quotation marked as accepted. You can now convert it to an order.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var quotation = await _context.Quotations.FindAsync(id);
        if (quotation is null) return NotFound();

        quotation.Status = QuotationStatus.Rejected;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Quotation marked as rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Deliberate, separate step from Accept — creates the Order (and its
    /// OrderDetails) from this quotation's priced lines. Never fires
    /// automatically, so accepting a quotation never silently creates an
    /// order, and this can't be triggered twice by refreshing the page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToOrder(int id)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Details)
            .FirstOrDefaultAsync(q => q.QuotationId == id);
        if (quotation is null) return NotFound();

        if (quotation.Status != QuotationStatus.Accepted)
        {
            TempData["Error"] = "Only an accepted quotation can be converted to an order.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var subTotal = quotation.Details.Sum(d => d.TotalPrice ?? 0);

        var order = new Core.Entities.Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            CustomerId = quotation.CustomerId,
            QuotationId = quotation.QuotationId,
            DeliveryLocation = quotation.DeliveryLocation,
            OrderStatus = OrderStatus.Created,
            PaymentStatus = PaymentStatus.Pending,
            Subtotal = subTotal,
            Discount = quotation.DiscountAmount ?? 0,
            TotalAmount = quotation.TotalAmount ?? 0
        };

        foreach (var detail in quotation.Details)
        {
            order.Details.Add(new Core.Entities.OrderDetail
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice ?? 0,
                TotalPrice = detail.TotalPrice ?? 0
            });
        }

        order.StatusHistory.Add(new Core.Entities.OrderStatusHistory
        {
            Status = OrderStatus.Created,
            Notes = $"Created from quotation {quotation.QuotationNumber}."
        });

        _context.Orders.Add(order);
        quotation.Status = QuotationStatus.ConvertedToOrder;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Order {order.OrderNumber} created.";
        return RedirectToAction("Details", "Orders", new { id = order.OrderId });
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var countThisYear = await _context.Orders.CountAsync(o => o.OrderDate.Year == year);
        return $"MED-{year}-{(countThisYear + 1):D6}";
    }

    private Task<Core.Entities.Quotation?> LoadQuotationAsync(int id) =>
        _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(q => q.QuotationId == id);

    private static QuotationPricingViewModel ToViewModel(Core.Entities.Quotation quotation) => new()
    {
        QuotationId = quotation.QuotationId,
        QuotationNumber = quotation.QuotationNumber,
        Status = quotation.Status,
        CustomerName = (quotation.Customer.FirstName + " " + quotation.Customer.LastName).Trim(),
        OrganizationName = quotation.Customer.OrganizationName,
        CustomerEmail = quotation.Customer.Email,
        CustomerPhone = quotation.Customer.Phone,
        RequestDate = quotation.RequestDate,
        CustomerNotes = quotation.CustomerNotes,
        DeliveryLocation = quotation.DeliveryLocation,
        DiscountAmount = quotation.DiscountAmount,
        ValidUntil = quotation.ValidUntil,
        AdminNotes = quotation.AdminNotes,
        Lines = quotation.Details.Select(d => new QuotationLinePricingViewModel
        {
            QuotationDetailId = d.QuotationDetailId,
            ProductId = d.ProductId,
            ProductName = d.Product.ProductName,
            PackSize = d.Product.PackSize,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice
        }).ToList()
    };
}
