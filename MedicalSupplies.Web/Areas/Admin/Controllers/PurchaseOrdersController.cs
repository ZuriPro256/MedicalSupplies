using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
public class PurchaseOrdersController : Controller
{
    // Same whitelisted-transition pattern as Orders — no arbitrary status jumps,
    // and PartiallyReceived/Received are never reachable directly: only
    // ReceiveLine (by actually creating InventoryBatches) can set them.
    private static readonly Dictionary<PurchaseOrderStatus, PurchaseOrderStatus[]> AllowedTransitions = new()
    {
        [PurchaseOrderStatus.Draft] = new[] { PurchaseOrderStatus.Submitted, PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.Submitted] = new[] { PurchaseOrderStatus.Approved, PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.Approved] = new[] { PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.PartiallyReceived] = new[] { PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.Received] = Array.Empty<PurchaseOrderStatus>(),
        [PurchaseOrderStatus.Cancelled] = Array.Empty<PurchaseOrderStatus>()
    };

    private readonly ApplicationDbContext _context;

    public PurchaseOrdersController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(PurchaseOrderStatus? status)
    {
        var query = _context.PurchaseOrders.Include(po => po.Supplier).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(po => po.Status == status.Value);
        }

        var orders = await query
            .OrderByDescending(po => po.OrderDate)
            .Select(po => new PurchaseOrderListItemViewModel
            {
                PurchaseOrderId = po.PurchaseOrderId,
                PurchaseOrderNumber = po.PurchaseOrderNumber,
                SupplierName = po.Supplier.SupplierName,
                OrderDate = po.OrderDate,
                TotalAmount = po.TotalAmount,
                Currency = po.Currency,
                Status = po.Status
            })
            .ToListAsync();

        ViewBag.SelectedStatus = status;
        return View(orders);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new PurchaseOrderFormViewModel
        {
            SupplierOptions = await GetSupplierOptionsAsync(),
            ProductOptions = await GetProductOptionsAsync(),
            Lines = new List<PurchaseOrderLineFormViewModel> { new(), new() }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrderFormViewModel vm)
    {
        var lines = (vm.Lines ?? new()).Where(l => l.ProductId > 0 && l.Quantity > 0).ToList();
        if (lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one product line.");
        }

        if (!ModelState.IsValid)
        {
            vm.SupplierOptions = await GetSupplierOptionsAsync();
            vm.ProductOptions = await GetProductOptionsAsync();
            return View(vm);
        }

        var subtotal = lines.Sum(l => l.Quantity * l.UnitCost);

        var order = new PurchaseOrder
        {
            PurchaseOrderNumber = await GeneratePoNumberAsync(),
            SupplierId = vm.SupplierId,
            ExpectedDeliveryDate = vm.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Draft,
            Subtotal = subtotal,
            DeliveryCost = vm.DeliveryCost,
            TaxAmount = vm.TaxAmount,
            TotalAmount = subtotal + (vm.DeliveryCost ?? 0) + (vm.TaxAmount ?? 0),
            Currency = vm.Currency,
            Notes = vm.Notes,
            CreatedBy = User.Identity?.Name
        };

        foreach (var line in lines)
        {
            order.Details.Add(new PurchaseOrderDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                TotalCost = line.Quantity * line.UnitCost
            });
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Purchase order {order.PurchaseOrderNumber} saved as Draft.";
        return RedirectToAction(nameof(Details), new { id = order.PurchaseOrderId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await _context.PurchaseOrders.Include(po => po.Details).FirstOrDefaultAsync(po => po.PurchaseOrderId == id);
        if (order is null) return NotFound();
        if (order.Status != PurchaseOrderStatus.Draft)
        {
            TempData["Error"] = "Only a Draft purchase order can be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var vm = new PurchaseOrderFormViewModel
        {
            SupplierId = order.SupplierId,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            DeliveryCost = order.DeliveryCost,
            TaxAmount = order.TaxAmount,
            Currency = order.Currency,
            Notes = order.Notes,
            Lines = order.Details.Select(d => new PurchaseOrderLineFormViewModel
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost
            }).ToList(),
            SupplierOptions = await GetSupplierOptionsAsync(),
            ProductOptions = await GetProductOptionsAsync()
        };
        if (vm.Lines.Count == 0) vm.Lines.Add(new());

        ViewBag.PurchaseOrderId = id;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PurchaseOrderFormViewModel vm)
    {
        var order = await _context.PurchaseOrders.Include(po => po.Details).FirstOrDefaultAsync(po => po.PurchaseOrderId == id);
        if (order is null) return NotFound();
        if (order.Status != PurchaseOrderStatus.Draft)
        {
            TempData["Error"] = "Only a Draft purchase order can be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var lines = (vm.Lines ?? new()).Where(l => l.ProductId > 0 && l.Quantity > 0).ToList();
        if (lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one product line.");
        }

        if (!ModelState.IsValid)
        {
            vm.SupplierOptions = await GetSupplierOptionsAsync();
            vm.ProductOptions = await GetProductOptionsAsync();
            ViewBag.PurchaseOrderId = id;
            return View(vm);
        }

        order.SupplierId = vm.SupplierId;
        order.ExpectedDeliveryDate = vm.ExpectedDeliveryDate;
        order.DeliveryCost = vm.DeliveryCost;
        order.TaxAmount = vm.TaxAmount;
        order.Currency = vm.Currency;
        order.Notes = vm.Notes;

        // Draft lines are replaced wholesale — nothing's been received yet,
        // so there's no QuantityReceived history to preserve.
        _context.PurchaseOrderDetails.RemoveRange(order.Details);
        order.Details.Clear();
        foreach (var line in lines)
        {
            order.Details.Add(new PurchaseOrderDetail
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                TotalCost = line.Quantity * line.UnitCost
            });
        }

        var subtotal = lines.Sum(l => l.Quantity * l.UnitCost);
        order.Subtotal = subtotal;
        order.TotalAmount = subtotal + (vm.DeliveryCost ?? 0) + (vm.TaxAmount ?? 0);

        await _context.SaveChangesAsync();
        TempData["Success"] = "Purchase order updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await LoadOrderAsync(id);
        if (order is null) return NotFound();
        return View(ToViewModel(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, PurchaseOrderStatus newStatus)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order is null) return NotFound();

        if (!AllowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(newStatus))
        {
            TempData["Error"] = $"Can't move a purchase order from {order.Status} to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = newStatus;
        if (newStatus == PurchaseOrderStatus.Approved)
        {
            order.ApprovedBy = User.Identity?.Name;
            order.ApprovedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Purchase order moved to {newStatus}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(int id, PaymentStatus paymentStatus)
    {
        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order is null) return NotFound();

        order.PaymentStatus = paymentStatus;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Payment status updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> ReceiveLine(int id)
    {
        var line = await _context.PurchaseOrderDetails
            .Include(d => d.Product)
            .Include(d => d.PurchaseOrder).ThenInclude(po => po.Supplier)
            .FirstOrDefaultAsync(d => d.PurchaseOrderDetailId == id);
        if (line is null) return NotFound();

        if (line.PurchaseOrder.Status is not (PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived))
        {
            TempData["Error"] = "Goods can only be received against an Approved (or partially received) purchase order.";
            return RedirectToAction(nameof(Details), new { id = line.PurchaseOrderId });
        }
        if (line.QuantityRemaining <= 0)
        {
            TempData["Error"] = "This line has already been received in full.";
            return RedirectToAction(nameof(Details), new { id = line.PurchaseOrderId });
        }

        return View(new ReceiveLineViewModel
        {
            PurchaseOrderDetailId = line.PurchaseOrderDetailId,
            PurchaseOrderId = line.PurchaseOrderId,
            PurchaseOrderNumber = line.PurchaseOrder.PurchaseOrderNumber,
            SupplierName = line.PurchaseOrder.Supplier.SupplierName,
            ProductName = line.Product.ProductName,
            Quantity = line.Quantity,
            QuantityReceived = line.QuantityReceived
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReceiveLine(int id, ReceiveLineViewModel vm)
    {
        var line = await _context.PurchaseOrderDetails
            .Include(d => d.Product)
            .Include(d => d.PurchaseOrder)
            .FirstOrDefaultAsync(d => d.PurchaseOrderDetailId == id);
        if (line is null) return NotFound();

        if (line.PurchaseOrder.Status is not (PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived))
        {
            TempData["Error"] = "Goods can only be received against an Approved (or partially received) purchase order.";
            return RedirectToAction(nameof(Details), new { id = line.PurchaseOrderId });
        }

        var batchRows = (vm.Batches ?? new()).Where(b => b.Quantity > 0).ToList();
        var totalNow = batchRows.Sum(b => b.Quantity);

        // The important check: what's being received now must fit within
        // what's still outstanding on this line — verified server-side,
        // not just hinted at client-side.
        if (totalNow <= 0)
        {
            ModelState.AddModelError(string.Empty, "Enter at least one batch with a quantity greater than zero.");
        }
        else if (totalNow > line.QuantityRemaining)
        {
            ModelState.AddModelError(string.Empty,
                $"You're receiving {totalNow}, but only {line.QuantityRemaining} is still outstanding on this line.");
        }
        if (batchRows.Any(b => string.IsNullOrWhiteSpace(b.BatchNumber)))
        {
            ModelState.AddModelError(string.Empty, "Every batch row needs a batch number.");
        }

        if (!ModelState.IsValid)
        {
            vm.PurchaseOrderDetailId = line.PurchaseOrderDetailId;
            vm.PurchaseOrderId = line.PurchaseOrderId;
            vm.ProductName = line.Product.ProductName;
            vm.Quantity = line.Quantity;
            vm.QuantityReceived = line.QuantityReceived;
            if (vm.Batches is null || !vm.Batches.Any()) vm.Batches = new() { new(), new() };
            return View(vm);
        }

        var changedBy = User.Identity?.Name;

        foreach (var row in batchRows)
        {
            var batch = new InventoryBatch
            {
                ProductId = line.ProductId,
                SupplierId = line.PurchaseOrder.SupplierId,
                PurchaseOrderId = line.PurchaseOrderId,
                BatchNumber = row.BatchNumber,
                ExpiryDate = row.ExpiryDate,
                QuantityReceived = row.Quantity,
                QuantityAvailable = row.Quantity,
                PurchasePrice = line.UnitCost,
                Status = BatchStatus.Active
            };
            _context.InventoryBatches.Add(batch);

            _context.StockMovements.Add(new StockMovement
            {
                ProductId = line.ProductId,
                Batch = batch,
                MovementType = StockMovementType.PurchaseReceipt,
                QuantityChange = row.Quantity,
                PurchaseOrderId = line.PurchaseOrderId,
                Notes = $"Received against {line.PurchaseOrder.PurchaseOrderNumber}.",
                CreatedBy = changedBy
            });
        }

        line.QuantityReceived += totalNow;

        // Receiving is what actually moves the PO's status — never a
        // manual "mark as received" button. line.QuantityReceived is
        // already updated above, and EF's identity map guarantees this
        // query returns that same tracked instance for this line, so a
        // plain check here (no extra arithmetic) is correct for every line.
        var allLines = await _context.PurchaseOrderDetails.Where(d => d.PurchaseOrderId == line.PurchaseOrderId).ToListAsync();
        var willBeFullyReceived = allLines.All(d => d.QuantityReceived >= d.Quantity);

        line.PurchaseOrder.Status = willBeFullyReceived
            ? PurchaseOrderStatus.Received
            : PurchaseOrderStatus.PartiallyReceived;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Received {totalNow} across {batchRows.Count} batch(es).";
        return RedirectToAction(nameof(Details), new { id = line.PurchaseOrderId });
    }

    private async Task<string> GeneratePoNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var countThisYear = await _context.PurchaseOrders.CountAsync(po => po.OrderDate.Year == year);
        return $"PO-{year}-{(countThisYear + 1):D4}";
    }

    private Task<PurchaseOrder?> LoadOrderAsync(int id) =>
        _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(po => po.PurchaseOrderId == id);

    private static PurchaseOrderDetailsViewModel ToViewModel(PurchaseOrder order) => new()
    {
        PurchaseOrderId = order.PurchaseOrderId,
        PurchaseOrderNumber = order.PurchaseOrderNumber,
        Status = order.Status,
        SupplierName = order.Supplier.SupplierName,
        SupplierContact = order.Supplier.ContactPerson,
        OrderDate = order.OrderDate,
        ExpectedDeliveryDate = order.ExpectedDeliveryDate,
        Lines = order.Details.Select(d => new PurchaseOrderLineViewModel
        {
            PurchaseOrderDetailId = d.PurchaseOrderDetailId,
            ProductName = d.Product.ProductName,
            Quantity = d.Quantity,
            UnitCost = d.UnitCost,
            TotalCost = d.TotalCost,
            QuantityReceived = d.QuantityReceived
        }).ToList(),
        Subtotal = order.Subtotal,
        DeliveryCost = order.DeliveryCost,
        TaxAmount = order.TaxAmount,
        TotalAmount = order.TotalAmount,
        Currency = order.Currency,
        PaymentStatus = order.PaymentStatus,
        Notes = order.Notes,
        CreatedBy = order.CreatedBy,
        ApprovedBy = order.ApprovedBy,
        ApprovedDate = order.ApprovedDate,
        AvailableNextStatuses = AllowedTransitions.TryGetValue(order.Status, out var next) ? next.ToList() : new(),
        CanReceive = order.Status is PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived
    };

    private async Task<List<SelectListItemModel>> GetSupplierOptionsAsync() =>
        await _context.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.SupplierName)
            .Select(s => new SelectListItemModel { Value = s.SupplierId, Text = s.SupplierName })
            .ToListAsync();

    private async Task<List<SelectListItemModel>> GetProductOptionsAsync() =>
        await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductName)
            .Select(p => new SelectListItemModel { Value = p.ProductId, Text = p.ProductName + " (" + p.ProductCode + ")" })
            .ToListAsync();
}
