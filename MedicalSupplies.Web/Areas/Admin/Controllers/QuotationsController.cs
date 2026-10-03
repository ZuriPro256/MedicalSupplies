using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.Services;
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
    private readonly ICustomerNotificationService _customerNotificationService;

    public QuotationsController(
        ApplicationDbContext context,
        ICustomerNotificationService customerNotificationService)
    {
        _context = context;
        _customerNotificationService = customerNotificationService;
    }

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
            var detail = quotation.Details.FirstOrDefault(
                d => d.QuotationDetailId == line.QuotationDetailId);

            if (detail is null)
            {
                TempData["Error"] = "The quotation contains an invalid line item. Please reload the quotation and try again.";
                return RedirectToAction(nameof(Details), new { id = vm.QuotationId });
            }

            detail.UnitPrice = line.UnitPrice;
            detail.TotalPrice = (line.UnitPrice ?? 0) * detail.Quantity;
        }

        quotation.DeliveryLocation = vm.DeliveryLocation;
        quotation.DiscountAmount = vm.DiscountAmount;
        quotation.ValidUntil = vm.ValidUntil;
        quotation.AdminNotes = vm.AdminNotes;

        var subTotal = quotation.Details.Sum(d => d.TotalPrice ?? 0);
        quotation.TotalAmount = Math.Max(0, subTotal - (vm.DiscountAmount ?? 0));

        var wasAlreadySent = quotation.Status == QuotationStatus.Sent;

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

        if (submitAction == "send")
        {
            var customerUserId = await _context.Customers
                .Where(c => c.CustomerId == quotation.CustomerId)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(customerUserId))
            {
                await _customerNotificationService.CreateAsync(
                    customerUserId,
                    wasAlreadySent ? "QuotationUpdated" : "QuotationSent",
                    wasAlreadySent ? "Quotation updated" : "Quotation ready",
                    wasAlreadySent
                        ? $"Quotation {quotation.QuotationNumber} has been updated and sent to you."
                        : $"Quotation {quotation.QuotationNumber} has been sent to you.",
                    $"/Account/QuotationDetails/{quotation.QuotationId}");
            }
        }

        TempData["Success"] = submitAction == "send" ? "Quotation marked as sent." : "Quotation saved.";
        return RedirectToAction(nameof(Details), new { id = vm.QuotationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRevisedOffer(int id)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Details)
            .FirstOrDefaultAsync(q => q.QuotationId == id);

        if (quotation is null) return NotFound();

        if (quotation.Status != QuotationStatus.Rejected)
        {
            TempData["Error"] = "A revised offer can only be created for a rejected quotation.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var previousOffers = await _context.QuotationOffers
            .Where(o => o.QuotationId == id
                && (o.Status == QuotationOfferStatus.Draft
                    || o.Status == QuotationOfferStatus.Sent))
            .ToListAsync();

        foreach (var previousOffer in previousOffers)
        {
            previousOffer.Status = QuotationOfferStatus.Superseded;
        }

        var nextRevision = (await _context.QuotationOffers
            .Where(o => o.QuotationId == id)
            .Select(o => (int?)o.RevisionNumber)
            .MaxAsync() ?? 0) + 1;

        var offer = new Core.Entities.QuotationOffer
        {
            QuotationId = quotation.QuotationId,
            RevisionNumber = nextRevision,
            Status = QuotationOfferStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            PreparedBy = User.Identity?.Name,
            DiscountAmount = quotation.DiscountAmount,
            TotalAmount = quotation.TotalAmount,
            AdminNotes = quotation.AdminNotes,
            ValidUntil = quotation.ValidUntil,
            OfferNumber = $"{quotation.QuotationNumber}-R{nextRevision}"
        };

        foreach (var detail in quotation.Details)
        {
            offer.Details.Add(new Core.Entities.QuotationOfferDetail
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                TotalPrice = detail.TotalPrice
            });
        }

        _context.QuotationOffers.Add(offer);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Revised offer {offer.OfferNumber} created.";
        return RedirectToAction(nameof(OfferDetails), new { id = offer.QuotationOfferId });
    }

    public async Task<IActionResult> EditOffer(int id)
    {
        var offer = await _context.QuotationOffers
            .Include(o => o.Quotation)
                .ThenInclude(q => q.Customer)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.QuotationOfferId == id);

        if (offer is null) return NotFound();

        if (offer.Status != QuotationOfferStatus.Draft)
        {
            TempData["Error"] = "Only a draft revised offer can be edited.";
            return RedirectToAction(nameof(OfferDetails), new { id });
        }

        var customerName = offer.Quotation.Customer.OrganizationName
            ?? $"{offer.Quotation.Customer.FirstName} {offer.Quotation.Customer.LastName}".Trim();

        var vm = new QuotationOfferEditViewModel
        {
            QuotationOfferId = offer.QuotationOfferId,
            QuotationId = offer.QuotationId,
            OfferNumber = offer.OfferNumber,
            RevisionNumber = offer.RevisionNumber,
            CustomerName = customerName,
            DiscountAmount = offer.DiscountAmount,
            DeliveryCost = offer.DeliveryCost,
            ValidUntil = offer.ValidUntil,
            AdminNotes = offer.AdminNotes,
            Lines = offer.Details.Select(d => new QuotationOfferLineEditViewModel
            {
                QuotationOfferDetailId = d.QuotationOfferDetailId,
                ProductId = d.ProductId,
                ProductName = d.Product.ProductName,
                PackSize = d.Product.PackSize,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOffer(QuotationOfferEditViewModel vm)
    {
        var offer = await _context.QuotationOffers
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.QuotationOfferId == vm.QuotationOfferId);

        if (offer is null) return NotFound();

        if (offer.Status != QuotationOfferStatus.Draft)
        {
            TempData["Error"] = "Only a draft revised offer can be edited.";
            return RedirectToAction(nameof(OfferDetails), new { id = vm.QuotationOfferId });
        }

        if (vm.DeliveryCost < 0)
        {
            ModelState.AddModelError(nameof(vm.DeliveryCost),
                "Delivery/transport cost cannot be negative.");
        }

        if (vm.DiscountAmount < 0)
        {
            ModelState.AddModelError(nameof(vm.DiscountAmount),
                "Discount cannot be negative.");
        }

        foreach (var line in vm.Lines)
        {
            if (line.Quantity <= 0)
            {
                ModelState.AddModelError(
                    $"Lines[{vm.Lines.IndexOf(line)}].Quantity",
                    "Quantity must be greater than zero.");
            }

            if (line.UnitPrice < 0)
            {
                ModelState.AddModelError(
                    $"Lines[{vm.Lines.IndexOf(line)}].UnitPrice",
                    "Unit price cannot be negative.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(EditOffer), vm);
        }

        foreach (var line in vm.Lines)
        {
            var detail = offer.Details.FirstOrDefault(
                d => d.QuotationOfferDetailId == line.QuotationOfferDetailId);

            if (detail is null)
            {
                TempData["Error"] =
                    "The revised offer contains an invalid line item. Please reload it and try again.";

                return RedirectToAction(nameof(EditOffer),
                    new { id = vm.QuotationOfferId });
            }

            detail.Quantity = line.Quantity;
            detail.UnitPrice = line.UnitPrice;
            detail.TotalPrice = (line.UnitPrice ?? 0) * line.Quantity;
        }

        var subTotal = offer.Details.Sum(d => d.TotalPrice ?? 0);
        var discount = vm.DiscountAmount ?? 0;
        var deliveryCost = vm.DeliveryCost;

        offer.DiscountAmount = discount;
        offer.DeliveryCost = deliveryCost;
        offer.TotalAmount = Math.Max(0, subTotal - discount + deliveryCost);
        offer.ValidUntil = vm.ValidUntil;
        offer.AdminNotes = vm.AdminNotes;
        offer.PreparedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            $"Revised offer {offer.OfferNumber} saved as draft.";

        return RedirectToAction(nameof(OfferDetails),
            new { id = offer.QuotationOfferId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOffer(int id)
    {
        var offer = await _context.QuotationOffers
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.QuotationOfferId == id);

        if (offer is null) return NotFound();

        if (offer.Status != QuotationOfferStatus.Draft)
        {
            TempData["Error"] = "Only a draft revised offer can be sent.";
            return RedirectToAction(nameof(OfferDetails), new { id });
        }

        if (!offer.Details.Any())
        {
            TempData["Error"] = "This revised offer has no line items and cannot be sent.";
            return RedirectToAction(nameof(OfferDetails), new { id });
        }

        offer.Status = QuotationOfferStatus.Sent;
        offer.SentDate = DateTime.UtcNow;
        offer.PreparedBy ??= User.Identity?.Name;

        await _context.SaveChangesAsync();

        var quotationInfo = await _context.Quotations
            .Where(q => q.QuotationId == offer.QuotationId)
            .Select(q => new
            {
                q.QuotationNumber,
                CustomerUserId = q.Customer.UserId
            })
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(quotationInfo?.CustomerUserId))
        {
            await _customerNotificationService.CreateAsync(
                quotationInfo.CustomerUserId,
                "RevisedOfferSent",
                "Revised offer available",
                $"Quotation {quotationInfo.QuotationNumber} has a revised offer ({offer.OfferNumber}) available for you.",
                $"/Account/QuotationDetails/{offer.QuotationId}");
        }

        TempData["Success"] = $"Revised offer {offer.OfferNumber} has been sent.";
        return RedirectToAction(nameof(OfferDetails), new { id });
    }

    public async Task<IActionResult> OfferDetails(int id)
    {
        var offer = await _context.QuotationOffers
            .Include(o => o.Quotation)
                .ThenInclude(q => q.Customer)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.QuotationOfferId == id);

        if (offer is null) return NotFound();

        return View(offer);
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
            .Include(q => q.AcceptedOffer)
                .ThenInclude(o => o!.Details)
            .FirstOrDefaultAsync(q => q.QuotationId == id);

        if (quotation is null) return NotFound();

        if (quotation.Status != QuotationStatus.Accepted)
        {
            TempData["Error"] = "Only an accepted quotation can be converted to an order.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (quotation.AcceptedOfferId is null || quotation.AcceptedOffer is null)
        {
            TempData["Error"] = "This accepted quotation does not have an accepted offer.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var acceptedOffer = quotation.AcceptedOffer;

        if (acceptedOffer.Status != QuotationOfferStatus.Accepted)
        {
            TempData["Error"] = "The quotation's selected offer is not marked as accepted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!acceptedOffer.Details.Any())
        {
            TempData["Error"] = "The accepted offer has no line items and cannot be converted to an order.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var subTotal = acceptedOffer.Details.Sum(d => d.TotalPrice ?? 0);
        var discount = acceptedOffer.DiscountAmount ?? 0;
        var deliveryCost = acceptedOffer.DeliveryCost;
        var totalAmount = acceptedOffer.TotalAmount ?? Math.Max(0, subTotal - discount + deliveryCost);

        var order = new Core.Entities.Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            CustomerId = quotation.CustomerId,
            QuotationId = quotation.QuotationId,
            DeliveryLocation = quotation.DeliveryLocation,
            OrderStatus = OrderStatus.Created,
            PaymentStatus = PaymentStatus.Pending,
            Subtotal = subTotal,
            Discount = discount,
            DeliveryCost = deliveryCost,
            TotalAmount = totalAmount
        };

        foreach (var detail in acceptedOffer.Details)
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
            Notes = $"Created from quotation {quotation.QuotationNumber}, accepted offer {acceptedOffer.OfferNumber}."
        });

        _context.Orders.Add(order);
        quotation.Status = QuotationStatus.ConvertedToOrder;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Order {order.OrderNumber} created from accepted offer {acceptedOffer.OfferNumber}.";
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
            .Include(q => q.Offers)
                .ThenInclude(o => o.Details)
                    .ThenInclude(d => d.Product)
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
        Offers = quotation.Offers
            .OrderByDescending(o => o.RevisionNumber)
            .Select(o => new QuotationOfferSummaryViewModel
            {
                QuotationOfferId = o.QuotationOfferId,
                OfferNumber = o.OfferNumber,
                RevisionNumber = o.RevisionNumber,
                Status = o.Status,
                CreatedDate = o.CreatedDate,
                SentDate = o.SentDate,
                TotalAmount = o.TotalAmount
            })
            .ToList(),
        AcceptedOffer = quotation.AcceptedOffer is not null
            ? new QuotationAcceptedOfferViewModel
            {
                QuotationOfferId = quotation.AcceptedOffer.QuotationOfferId,
                OfferNumber = quotation.AcceptedOffer.OfferNumber,
                RevisionNumber = quotation.AcceptedOffer.RevisionNumber,
                Lines = quotation.AcceptedOffer.Details
                    .Select(d => new QuotationAcceptedOfferLineViewModel
                    {
                        ProductName = d.Product.ProductName,
                        PackSize = d.Product.PackSize,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice ?? 0,
                        LineTotal = d.TotalPrice ?? ((d.UnitPrice ?? 0) * d.Quantity)
                    })
                    .ToList(),
                SubTotal = quotation.AcceptedOffer.Details.Sum(d => d.TotalPrice ?? 0),
                DiscountAmount = quotation.AcceptedOffer.DiscountAmount ?? 0,
                DeliveryCost = quotation.AcceptedOffer.DeliveryCost,
                GrandTotal = quotation.AcceptedOffer.TotalAmount
                    ?? Math.Max(
                        0,
                        quotation.AcceptedOffer.Details.Sum(d => d.TotalPrice ?? 0)
                        - (quotation.AcceptedOffer.DiscountAmount ?? 0)
                        + quotation.AcceptedOffer.DeliveryCost)
            }
            : null,
        RejectionReason = quotation.RejectionReason,
        CustomerExpectedPrice = quotation.CustomerExpectedPrice,
        CustomerRejectionComment = quotation.CustomerRejectionComment,
        RejectedDate = quotation.RejectedDate,
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
