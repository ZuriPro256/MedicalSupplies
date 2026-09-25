using MedicalSupplies.Core.Entities;
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
public class OrdersController : Controller
{
    // Only these transitions are allowed — no arbitrary status jumps.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Created] = new[] { OrderStatus.Processing, OrderStatus.Cancelled },
        [OrderStatus.Processing] = new[] { OrderStatus.Dispatched, OrderStatus.Cancelled },
        [OrderStatus.Dispatched] = new[] { OrderStatus.Delivered },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

    private readonly ApplicationDbContext _context;
    private readonly IOrderFulfillmentService _fulfillment;

    public OrdersController(ApplicationDbContext context, IOrderFulfillmentService fulfillment)
    {
        _context = context;
        _fulfillment = fulfillment;
    }

    public async Task<IActionResult> Index(OrderFilterViewModel filter)
    {
        var query = _context.Orders.Include(o => o.Customer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
        {
            query = query.Where(o => o.OrderNumber.Contains(filter.OrderNumber));
        }
        if (!string.IsNullOrWhiteSpace(filter.Customer))
        {
            query = query.Where(o =>
                (o.Customer.OrganizationName != null && o.Customer.OrganizationName.Contains(filter.Customer)) ||
                (o.Customer.FirstName != null && o.Customer.FirstName.Contains(filter.Customer)) ||
                (o.Customer.LastName != null && o.Customer.LastName.Contains(filter.Customer)));
        }
        if (filter.Status.HasValue)
        {
            query = query.Where(o => o.OrderStatus == filter.Status.Value);
        }
        if (filter.PaymentStatus.HasValue)
        {
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);
        }
        if (filter.FromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(filter.FromDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(o => o.OrderDate >= from);
        }
        if (filter.ToDate.HasValue)
        {
            var to = DateTime.SpecifyKind(filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
            query = query.Where(o => o.OrderDate <= to);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderListItemViewModel
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer.OrganizationName ?? (o.Customer.FirstName + " " + o.Customer.LastName),
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        ViewBag.Filter = filter;
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await LoadOrderAsync(id);
        if (order is null) return NotFound();

        return View(ToViewModel(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, OrderStatus newStatus, string? notes)
    {
        var order = await LoadOrderAsync(id);
        if (order is null) return NotFound();

        if (!AllowedTransitions.TryGetValue(order.OrderStatus, out var allowed) || !allowed.Contains(newStatus))
        {
            TempData["Error"] = $"Can't move an order from {order.OrderStatus} to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var changedBy = User.Identity?.IsAuthenticated == true ? User.Identity!.Name : null;

        // Stock is deducted here — the moment fulfilment is confirmed — never at
        // quotation creation and never at order creation, so an order that's
        // still sitting at "Created" (or gets cancelled before Processing)
        // never touches stock figures at all.
        if (newStatus == OrderStatus.Processing)
        {
            var allocation = await _fulfillment.DeductStockForOrderAsync(order, changedBy);
            if (!allocation.Success)
            {
                TempData["Error"] = "Not enough stock to process this order: " + string.Join("; ", allocation.InsufficientProducts);
                return RedirectToAction(nameof(Details), new { id });
            }
        }
        else if (newStatus == OrderStatus.Cancelled && order.OrderStatus == OrderStatus.Processing)
        {
            // Only Processing has actually deducted stock (Dispatched can't be
            // cancelled per the allowed-transitions map above).
            await _fulfillment.ReverseStockForOrderAsync(order, changedBy);
        }

        if (newStatus == OrderStatus.Dispatched)
        {
            order.DispatchedDate = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Delivered)
        {
            order.DeliveredDate = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Cancelled)
        {
            order.DeliveryNotes = notes;
        }

        order.OrderStatus = newStatus;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = newStatus,
            ChangedBy = changedBy,
            Notes = notes
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Order moved to {newStatus}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(int id, PaymentStatus paymentStatus, decimal? amountPaid)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null) return NotFound();

        order.PaymentStatus = paymentStatus;
        order.AmountPaid = amountPaid ?? order.AmountPaid;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Payment status updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private Task<Order?> LoadOrderAsync(int id) =>
        _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Details).ThenInclude(d => d.Product)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.OrderId == id);

    private static OrderDetailsViewModel ToViewModel(Order order) => new()
    {
        OrderId = order.OrderId,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        CustomerName = (order.Customer.FirstName + " " + order.Customer.LastName).Trim(),
        OrganizationName = order.Customer.OrganizationName,
        CustomerPhone = order.Customer.Phone,
        CustomerEmail = order.Customer.Email,
        DeliveryLocation = order.DeliveryLocation,
        Lines = order.Details.Select(d => new OrderLineViewModel
        {
            ProductName = d.Product.ProductName,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            TotalPrice = d.TotalPrice
        }).ToList(),
        Subtotal = order.Subtotal,
        Discount = order.Discount,
        TotalAmount = order.TotalAmount,
        OrderStatus = order.OrderStatus,
        PaymentStatus = order.PaymentStatus,
        AmountPaid = order.AmountPaid,
        DeliveryNotes = order.DeliveryNotes,
        DispatchedDate = order.DispatchedDate,
        DeliveredDate = order.DeliveredDate,
        StatusHistory = order.StatusHistory
            .OrderByDescending(h => h.ChangedDate)
            .Select(h => new OrderStatusHistoryViewModel
            {
                Status = h.Status,
                ChangedDate = h.ChangedDate,
                ChangedBy = h.ChangedBy,
                Notes = h.Notes
            }).ToList(),
        AvailableNextStatuses = AllowedTransitions.TryGetValue(order.OrderStatus, out var next)
            ? next.ToList()
            : new List<OrderStatus>()
    };
}
