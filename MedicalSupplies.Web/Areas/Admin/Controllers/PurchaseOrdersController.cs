using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
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
        [PurchaseOrderStatus.Draft] = new[] { PurchaseOrderStatus.Submitted },
        [PurchaseOrderStatus.Submitted] = new[] { PurchaseOrderStatus.Approved },
        [PurchaseOrderStatus.Approved] = Array.Empty<PurchaseOrderStatus>(),
        [PurchaseOrderStatus.PartiallyReceived] = Array.Empty<PurchaseOrderStatus>(),
        [PurchaseOrderStatus.Received] = Array.Empty<PurchaseOrderStatus>(),
        [PurchaseOrderStatus.Cancelled] = Array.Empty<PurchaseOrderStatus>()
    };

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    private async Task<string?> GetCurrentUserDisplayNameAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return User.Identity?.Name;

        return string.IsNullOrWhiteSpace(user.FullName)
            ? user.UserName
            : user.FullName;
    }

    public PurchaseOrdersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

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
            Lines = new List<PurchaseOrderLineFormViewModel> { new() }
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
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var poYear = DateTime.UtcNow.Year;

                // Serialize PO number generation for the current year.
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(2026, {poYear})");

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

                _context.PurchaseOrderStatusHistories.Add(new PurchaseOrderStatusHistory
                {
                    PurchaseOrderId = order.PurchaseOrderId,
                    Status = PurchaseOrderStatus.Draft,
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = await GetCurrentUserDisplayNameAsync(),
                    Notes = "Purchase order created."
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Purchase order {order.PurchaseOrderNumber} saved as Draft.";

                return RedirectToAction(nameof(Details), new { id = order.PurchaseOrderId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
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

        var duplicateProductIds = lines
            .GroupBy(l => l.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateProductIds.Count > 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "The same product cannot appear more than once on a purchase order. " +
                "Combine the quantities into one line.");
        }

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

    [HttpGet]
    public async Task<IActionResult> Return(int id)
    {
        var order = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Details)
                .ThenInclude(d => d.Product)
            .Include(po => po.Batches)
                .ThenInclude(b => b.Product)
            .FirstOrDefaultAsync(po => po.PurchaseOrderId == id);

        if (order is null)
            return NotFound();

        if (order.Status is not (
            PurchaseOrderStatus.PartiallyReceived or
            PurchaseOrderStatus.Received))
        {
            TempData["Error"] =
                "Goods can only be returned from a partially received or received purchase order.";

            return RedirectToAction(nameof(Details), new { id });
        }

        var batches = order.Batches
            .Where(b => b.QuantityAvailable > 0)
            .Select(b =>
            {
                var detail = order.Details
                    .FirstOrDefault(d => d.ProductId == b.ProductId);

                return new PurchaseOrderReturnBatchViewModel
                {
                    BatchId = b.BatchId,
                    PurchaseOrderDetailId = detail?.PurchaseOrderDetailId ?? 0,
                    ProductName = b.Product.ProductName,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate,
                    QuantityReceived = b.QuantityReceived,
                    QuantityAvailable = b.QuantityAvailable
                };
            })
            .Where(b => b.PurchaseOrderDetailId > 0)
            .OrderBy(b => b.ProductName)
            .ThenBy(b => b.ExpiryDate)
            .ThenBy(b => b.BatchNumber)
            .ToList();

        if (batches.Count == 0)
        {
            TempData["Error"] =
                "There is no available inventory from this purchase order to return.";

            return RedirectToAction(nameof(Details), new { id });
        }

        return View(new PurchaseOrderReturnViewModel
        {
            PurchaseOrderId = order.PurchaseOrderId,
            PurchaseOrderNumber = order.PurchaseOrderNumber,
            SupplierName = order.Supplier.SupplierName,
            Batches = batches
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(
        PurchaseOrderReturnViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Reason))
        {
            TempData["Error"] = "A return reason is required.";
            return RedirectToAction(nameof(Return), new { id = model.PurchaseOrderId });
        }

        model.Reason = model.Reason.Trim();

        if (model.Reason.Length > 1000)
        {
            TempData["Error"] = "The return reason cannot exceed 1000 characters.";
            return RedirectToAction(nameof(Return), new { id = model.PurchaseOrderId });
        }

        if (model.Notes?.Length > 500)
        {
            TempData["Error"] = "Return notes cannot exceed 500 characters.";
            return RedirectToAction(nameof(Return), new { id = model.PurchaseOrderId });
        }

        var selectedBatches = model.Batches
            .Where(b => b.QuantityToReturn > 0)
            .ToList();

        var duplicateBatchIds = selectedBatches
            .GroupBy(b => b.BatchId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateBatchIds.Count > 0)
        {
            TempData["Error"] =
                "The same inventory batch cannot be submitted more than once in a goods return.";
            return RedirectToAction(nameof(Return), new { id = model.PurchaseOrderId });
        }

        if (selectedBatches.Count == 0)
        {
            TempData["Error"] = "Enter a quantity to return for at least one batch.";
            return RedirectToAction(nameof(Return), new { id = model.PurchaseOrderId });
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.PurchaseOrders
                    .FromSqlInterpolated($"""
                    SELECT *
                    FROM "PurchaseOrders"
                    WHERE "PurchaseOrderId" = {model.PurchaseOrderId}
                    FOR UPDATE
                    """)
                    .Include(po => po.Supplier)
                    .FirstOrDefaultAsync();

                if (order is null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                if (order.Status is not (
                    PurchaseOrderStatus.PartiallyReceived or
                    PurchaseOrderStatus.Received))
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "Goods can only be returned from a partially received or received purchase order.";

                    return RedirectToAction(nameof(Details),
                        new { id = model.PurchaseOrderId });
                }

                var batchIds = selectedBatches
                    .Select(b => b.BatchId)
                    .Distinct()
                    .ToList();

                // Lock the actual inventory rows before checking QuantityAvailable.
                // Order processing uses the same batch-level locks, so a supplier return
                // cannot race against an order consuming the same stock.
                var batches = await _context.InventoryBatches
                    .FromSqlInterpolated($"""
                    SELECT *
                    FROM "InventoryBatches"
                    WHERE "BatchId" = ANY({batchIds.ToArray()})
                      AND "PurchaseOrderId" = {order.PurchaseOrderId}
                    ORDER BY "BatchId"
                    FOR UPDATE
                    """)
                    .Include(b => b.Product)
                    .ToListAsync();

                if (batches.Count != batchIds.Count)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "One or more selected inventory batches do not belong to this purchase order.";

                    return RedirectToAction(nameof(Return),
                        new { id = model.PurchaseOrderId });
                }

                var detailIds = selectedBatches
                    .Select(b => b.PurchaseOrderDetailId)
                    .Distinct()
                    .ToList();

                var details = await _context.PurchaseOrderDetails
                    .Where(d =>
                        detailIds.Contains(d.PurchaseOrderDetailId) &&
                        d.PurchaseOrderId == order.PurchaseOrderId)
                    .ToListAsync();

                if (details.Count != detailIds.Count)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "One or more selected purchase-order lines are invalid.";

                    return RedirectToAction(nameof(Return),
                        new { id = model.PurchaseOrderId });
                }

                var batchById = batches.ToDictionary(b => b.BatchId);
                var detailById = details.ToDictionary(d => d.PurchaseOrderDetailId);

                var changedBy = await GetCurrentUserDisplayNameAsync();

                var returnRecord = new PurchaseOrderReturn
                {
                    PurchaseOrderId = order.PurchaseOrderId,
                    ReturnNumber = $"RET-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..40],
                    ReturnDate = DateTime.UtcNow,
                    Reason = model.Reason,
                    RecordedBy = changedBy,
                    Notes = string.IsNullOrWhiteSpace(model.Notes)
                        ? null
                        : model.Notes.Trim()
                };

                _context.PurchaseOrderReturns.Add(returnRecord);

                foreach (var selected in selectedBatches)
                {
                    if (!batchById.TryGetValue(selected.BatchId, out var batch))
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "An invalid inventory batch was selected.";
                        return RedirectToAction(nameof(Return),
                            new { id = model.PurchaseOrderId });
                    }

                    if (!detailById.TryGetValue(
                            selected.PurchaseOrderDetailId,
                            out var detail))
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "An invalid purchase-order line was selected.";
                        return RedirectToAction(nameof(Return),
                            new { id = model.PurchaseOrderId });
                    }

                    if (batch.ProductId != detail.ProductId)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] =
                            "The selected inventory batch does not match its purchase-order line.";

                        return RedirectToAction(nameof(Return),
                            new { id = model.PurchaseOrderId });
                    }

                    if (selected.QuantityToReturn > batch.QuantityAvailable)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            $"Cannot return {selected.QuantityToReturn} unit(s) from batch " +
                            $"{batch.BatchNumber ?? batch.BatchId.ToString()}. " +
                            $"Only {batch.QuantityAvailable} unit(s) are currently available.";

                        return RedirectToAction(nameof(Return),
                            new { id = model.PurchaseOrderId });
                    }

                    batch.QuantityAvailable -= selected.QuantityToReturn;

                    if (batch.QuantityAvailable == 0)
                        batch.Status = BatchStatus.Depleted;

                    returnRecord.Details.Add(new PurchaseOrderReturnDetail
                    {
                        PurchaseOrderDetailId = detail.PurchaseOrderDetailId,
                        BatchId = batch.BatchId,
                        QuantityReturned = selected.QuantityToReturn,
                        UnitCost = batch.PurchasePrice ?? detail.UnitCost,
                        Notes = null
                    });

                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = batch.ProductId,
                        BatchId = batch.BatchId,
                        MovementType = StockMovementType.Return,
                        QuantityChange = -selected.QuantityToReturn,
                        PurchaseOrderId = order.PurchaseOrderId,
                        Notes =
                            $"Supplier return {returnRecord.ReturnNumber} " +
                            $"against {order.PurchaseOrderNumber}.",
                        CreatedBy = changedBy
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Goods return {returnRecord.ReturnNumber} was recorded successfully.";

                return RedirectToAction(nameof(Details),
                    new { id = order.PurchaseOrderId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string cancellationReason)
    {
        if (string.IsNullOrWhiteSpace(cancellationReason))
        {
            TempData["Error"] = "A cancellation reason is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        cancellationReason = cancellationReason.Trim();

        if (cancellationReason.Length > 500)
        {
            TempData["Error"] = "The cancellation reason cannot exceed 500 characters.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.PurchaseOrders
                    .FromSqlInterpolated($"""
                    SELECT *
                    FROM "PurchaseOrders"
                    WHERE "PurchaseOrderId" = {id}
                    FOR UPDATE
                    """)
                    .SingleOrDefaultAsync();

                if (order is null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                if (order.Status == PurchaseOrderStatus.Received)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "A received purchase order cannot be cancelled.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (order.Status == PurchaseOrderStatus.Cancelled)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "This purchase order is already cancelled.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var cancellableStatuses = new[]
                {
                PurchaseOrderStatus.Draft,
                PurchaseOrderStatus.Submitted,
                PurchaseOrderStatus.Approved
            };

                if (!cancellableStatuses.Contains(order.Status))
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] =
                        $"A purchase order in {order.Status} status cannot be cancelled.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (order.AmountPaid > 0)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] =
                        $"This purchase order cannot be cancelled because {order.Currency} {order.AmountPaid:N0} has already been paid to the supplier.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var previousStatus = order.Status;

                order.Status = PurchaseOrderStatus.Cancelled;

                _context.PurchaseOrderStatusHistories.Add(new PurchaseOrderStatusHistory
                {
                    PurchaseOrderId = order.PurchaseOrderId,
                    Status = PurchaseOrderStatus.Cancelled,
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = await GetCurrentUserDisplayNameAsync(),
                    Notes = $"Cancelled from {previousStatus}. Reason: {cancellationReason}"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Purchase order {order.PurchaseOrderNumber} was cancelled successfully.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] =
                    "The purchase order could not be cancelled. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Restore(int id)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.PurchaseOrders
                    .FromSqlInterpolated($"""
                    SELECT *
                    FROM "PurchaseOrders"
                    WHERE "PurchaseOrderId" = {id}
                    FOR UPDATE
                    """)
                    .SingleOrDefaultAsync();

                if (order is null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                if (order.Status != PurchaseOrderStatus.Cancelled)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] =
                        "Only a cancelled purchase order can be restored.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (order.AmountPaid > 0)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] =
                        "This purchase order cannot be restored because supplier payment has been recorded.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var cancellationHistory = await _context.PurchaseOrderStatusHistories
                    .Where(h =>
                        h.PurchaseOrderId == id &&
                        h.Status == PurchaseOrderStatus.Cancelled)
                    .OrderByDescending(h => h.PurchaseOrderStatusHistoryId)
                    .FirstOrDefaultAsync();

                if (cancellationHistory is null)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] =
                        "The cancellation history could not be found. The purchase order was not restored.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var previousHistory = await _context.PurchaseOrderStatusHistories
                    .Where(h =>
                        h.PurchaseOrderId == id &&
                        h.PurchaseOrderStatusHistoryId < cancellationHistory.PurchaseOrderStatusHistoryId)
                    .OrderByDescending(h => h.PurchaseOrderStatusHistoryId)
                    .FirstOrDefaultAsync();

                if (previousHistory is null ||
                    previousHistory.Status == PurchaseOrderStatus.Cancelled)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] =
                        "The previous purchase order status could not be determined. The purchase order was not restored.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var restoredStatus = previousHistory.Status;

                order.Status = restoredStatus;

                _context.PurchaseOrderStatusHistories.Add(new PurchaseOrderStatusHistory
                {
                    PurchaseOrderId = order.PurchaseOrderId,
                    Status = restoredStatus,
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = await GetCurrentUserDisplayNameAsync(),
                    Notes = $"Restored from Cancelled to {restoredStatus}."
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Purchase order {order.PurchaseOrderNumber} was restored to {restoredStatus}.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] =
                    "The purchase order could not be restored. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, PurchaseOrderStatus newStatus)
    {
        if (newStatus == PurchaseOrderStatus.Cancelled)
        {
            TempData["Error"] = "Use the cancellation action so a cancellation reason can be recorded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.PurchaseOrders
                .FromSqlInterpolated($"""
                    SELECT *
                    FROM "PurchaseOrders"
                    WHERE "PurchaseOrderId" = {id}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync();

            if (order is null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (!AllowedTransitions.TryGetValue(order.Status, out var allowed) ||
                !allowed.Contains(newStatus))
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Can't move a purchase order from {order.Status} to {newStatus}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var previousStatus = order.Status;
            order.Status = newStatus;

            if (newStatus == PurchaseOrderStatus.Approved)
            {
                order.ApprovedBy = User.Identity?.Name;
                order.ApprovedDate = DateTime.UtcNow;
            }

            _context.PurchaseOrderStatusHistories.Add(new PurchaseOrderStatusHistory
            {
                PurchaseOrderId = order.PurchaseOrderId,
                Status = newStatus,
                ChangedDate = DateTime.UtcNow,
                ChangedBy = await GetCurrentUserDisplayNameAsync(),
                Notes = $"Status changed from {previousStatus} to {newStatus}."
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"Purchase order moved to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "The purchase order status could not be changed. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(
        int id,
        decimal paymentAmount,
        DateTime paymentDate,
        string paymentMethod,
        string? referenceNumber,
        string? notes)
    {
        if (paymentAmount <= 0)
        {
            TempData["Error"] = "Payment amount must be greater than zero.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            TempData["Error"] = "Please select a payment method.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.PurchaseOrders
                .FromSqlInterpolated($"""
                    SELECT *
                    FROM "PurchaseOrders"
                    WHERE "PurchaseOrderId" = {id}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync();

            if (order is null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (order.Status == PurchaseOrderStatus.Cancelled)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "A cancelled purchase order cannot receive a supplier payment.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var remainingBalance = order.TotalAmount - order.AmountPaid;

            if (remainingBalance <= 0)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "This purchase order is already fully paid.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (paymentAmount > remainingBalance)
            {
                await transaction.RollbackAsync();
                TempData["Error"] =
                    $"Payment exceeds the remaining balance of {order.Currency} {remainingBalance:N0}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var payment = new SupplierPayment
            {
                PurchaseOrderId = order.PurchaseOrderId,
                Amount = paymentAmount,
                PaymentDate = paymentDate == default
                    ? DateTime.UtcNow
                    : TimeZoneInfo.ConvertTimeToUtc(
                        DateTime.SpecifyKind(paymentDate, DateTimeKind.Unspecified),
                        TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi")),
                PaymentMethod = paymentMethod.Trim(),
                ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber)
                    ? null
                    : referenceNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? null
                    : notes.Trim(),
                RecordedBy = (await _userManager.GetUserAsync(User)) is { } user
                ? (string.IsNullOrWhiteSpace(user.FullName)
                    ? user.UserName
                    : user.FullName)
                : User.Identity?.Name
            };

            _context.SupplierPayments.Add(payment);

            order.AmountPaid += paymentAmount;
            order.PaymentStatus = order.AmountPaid >= order.TotalAmount
                ? PaymentStatus.Paid
                : PaymentStatus.PartiallyPaid;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                $"Payment of {order.Currency} {paymentAmount:N0} recorded successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "The supplier payment could not be recorded. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
        });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReversePayment(
        int id,
        int paymentId,
        string? reversalReason)
    {
        if (string.IsNullOrWhiteSpace(reversalReason))
        {
            TempData["Error"] = "A reason is required when reversing a supplier payment.";
            return RedirectToAction(nameof(Details), new { id });
        }

        reversalReason = reversalReason.Trim();

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.PurchaseOrders
                .FromSqlInterpolated($"""
                    SELECT *
                    FROM "PurchaseOrders"
                    WHERE "PurchaseOrderId" = {id}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync();

            if (order is null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            var payment = await _context.SupplierPayments
                .SingleOrDefaultAsync(p =>
                    p.SupplierPaymentId == paymentId &&
                    p.PurchaseOrderId == id);

            if (payment is null)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "The supplier payment could not be found.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (payment.IsReversed)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "This supplier payment has already been reversed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var currentUser = await GetCurrentUserDisplayNameAsync();

            payment.IsReversed = true;
            payment.ReversedDate = DateTime.UtcNow;
            payment.ReversedBy = currentUser;
            payment.ReversalReason = reversalReason;

            order.AmountPaid = await _context.SupplierPayments
                .Where(p =>
                    p.PurchaseOrderId == id &&
                    !p.IsReversed)
                .SumAsync(p => p.Amount);

            order.PaymentStatus = order.AmountPaid >= order.TotalAmount
                ? PaymentStatus.Paid
                : order.AmountPaid > 0
                    ? PaymentStatus.PartiallyPaid
                    : PaymentStatus.Pending;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                $"Supplier payment of {order.Currency} {payment.Amount:N0} was reversed successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] =
                "The supplier payment could not be reversed. Please try again.";

            return RedirectToAction(nameof(Details), new { id });
        }
        });
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
        var lineInfo = await _context.PurchaseOrderDetails
            .AsNoTracking()
            .Where(d => d.PurchaseOrderDetailId == id)
            .Select(d => new { d.PurchaseOrderId })
            .FirstOrDefaultAsync();

        if (lineInfo is null) return NotFound();

        var batchRows = (vm.Batches ?? new()).Where(b => b.Quantity > 0).ToList();
        var totalNow = batchRows.Sum(b => b.Quantity);

        // Validate submitted batch data before acquiring the database lock.
        if (totalNow <= 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Enter at least one batch with a quantity greater than zero.");
        }

        if (batchRows.Any(b => string.IsNullOrWhiteSpace(b.BatchNumber)))
        {
            ModelState.AddModelError(
                string.Empty,
                "Every batch row needs a batch number.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));
        var expiredRows = batchRows
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value < today)
            .ToList();

        foreach (var row in expiredRows)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Batch {row.BatchNumber?.Trim()} has an expiry date of {row.ExpiryDate:dd MMM yyyy}, which has already passed.");
        }

        if (!ModelState.IsValid)
        {
            vm.PurchaseOrderDetailId = id;
            vm.PurchaseOrderId = lineInfo.PurchaseOrderId;

            var displayLine = await _context.PurchaseOrderDetails
                .AsNoTracking()
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.PurchaseOrderDetailId == id);

            if (displayLine is null) return NotFound();

            vm.ProductName = displayLine.Product.ProductName;
            vm.Quantity = displayLine.Quantity;
            vm.QuantityReceived = displayLine.QuantityReceived;

            if (vm.Batches is null || !vm.Batches.Any())
            {
                vm.Batches = new() { new(), new() };
            }

            return View(vm);
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lock the purchase order so concurrent receiving operations
                // against the same PO are serialized.
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                SELECT ""PurchaseOrderId""
                FROM ""PurchaseOrders""
                WHERE ""PurchaseOrderId"" = {lineInfo.PurchaseOrderId}
                FOR UPDATE");

                // Load the line AFTER acquiring the lock. Because the initial
                // lookup used AsNoTracking(), this is a fresh authoritative read.
                var line = await _context.PurchaseOrderDetails
                    .Include(d => d.Product)
                    .Include(d => d.PurchaseOrder)
                    .FirstOrDefaultAsync(d => d.PurchaseOrderDetailId == id);

                if (line is null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                if (line.PurchaseOrder.Status is not
                    (PurchaseOrderStatus.Approved or PurchaseOrderStatus.PartiallyReceived))
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "Goods can only be received against an Approved (or partially received) purchase order.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = line.PurchaseOrderId });
                }

                if (totalNow > line.QuantityRemaining)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        $"You're receiving {totalNow}, but only {line.QuantityRemaining} is still outstanding on this line.");

                    vm.PurchaseOrderDetailId = line.PurchaseOrderDetailId;
                    vm.PurchaseOrderId = line.PurchaseOrderId;
                    vm.ProductName = line.Product.ProductName;
                    vm.Quantity = line.Quantity;
                    vm.QuantityReceived = line.QuantityReceived;

                    return View(vm);
                }

                var changedBy = await GetCurrentUserDisplayNameAsync();
                var previousStatus = line.PurchaseOrder.Status;

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

                var allLines = await _context.PurchaseOrderDetails
                    .Where(d => d.PurchaseOrderId == line.PurchaseOrderId)
                    .ToListAsync();

                var willBeFullyReceived =
                    allLines.All(d => d.QuantityReceived >= d.Quantity);

                line.PurchaseOrder.Status = willBeFullyReceived
                    ? PurchaseOrderStatus.Received
                    : PurchaseOrderStatus.PartiallyReceived;


                if (line.PurchaseOrder.Status != previousStatus)
                {
                    _context.PurchaseOrderStatusHistories.Add(new PurchaseOrderStatusHistory
                    {
                        PurchaseOrderId = line.PurchaseOrderId,
                        Status = line.PurchaseOrder.Status,
                        ChangedBy = changedBy,
                        Notes = $"Received {totalNow} unit(s) across {batchRows.Count} batch(es)."
                    });
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Received {totalNow} across {batchRows.Count} batch(es).";

                return RedirectToAction(
                    nameof(Details),
                    new { id = line.PurchaseOrderId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task<string> GeneratePoNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PO-{year}-";

        var existingNumbers = await _context.PurchaseOrders
            .Where(po => po.PurchaseOrderNumber.StartsWith(prefix))
            .Select(po => po.PurchaseOrderNumber)
            .ToListAsync();

        var highestNumber = existingNumbers
            .Select(number => int.TryParse(number[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{(highestNumber + 1):D4}";
    }

    private Task<PurchaseOrder?> LoadOrderAsync(int id) =>
        _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Details).ThenInclude(d => d.Product)
            .Include(po => po.SupplierPayments)
            .Include(po => po.StatusHistory)
            .Include(po => po.Batches).ThenInclude(b => b.Product)
            .Include(po => po.Returns)
                .ThenInclude(r => r.Details)
                    .ThenInclude(d => d.Batch)
                        .ThenInclude(b => b.Product)
            .AsSplitQuery()
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
        AmountPaid = order.AmountPaid,
        SupplierPayments = order.SupplierPayments
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new SupplierPaymentViewModel
            {
                SupplierPaymentId = p.SupplierPaymentId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                Notes = p.Notes,
                RecordedBy = p.RecordedBy,
                IsReversed = p.IsReversed,
                ReversedDate = p.ReversedDate,
                ReversedBy = p.ReversedBy,
                ReversalReason = p.ReversalReason
            })
            .ToList(),
        StatusHistory = order.StatusHistory
            .OrderByDescending(h => h.ChangedDate)
            .ThenByDescending(h => h.PurchaseOrderStatusHistoryId)
            .Select(h => new PurchaseOrderStatusHistoryViewModel
            {
                PurchaseOrderStatusHistoryId = h.PurchaseOrderStatusHistoryId,
                Status = h.Status,
                ChangedDate = h.ChangedDate,
                ChangedBy = h.ChangedBy,
                Notes = h.Notes
            })
            .ToList(),
        Returns = order.Returns
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.PurchaseOrderReturnId)
            .Select(r => new PurchaseOrderReturnDetailsViewModel
            {
                PurchaseOrderReturnId = r.PurchaseOrderReturnId,
                ReturnNumber = r.ReturnNumber,
                ReturnDate = r.ReturnDate,
                Reason = r.Reason,
                RecordedBy = r.RecordedBy,
                Notes = r.Notes,
                Details = r.Details
                    .OrderBy(d => d.Batch.Product.ProductName)
                    .ThenBy(d => d.Batch.BatchNumber)
                    .Select(d => new PurchaseOrderReturnDetailViewModel
                    {
                        ProductName = d.Batch.Product.ProductName,
                        BatchNumber = d.Batch.BatchNumber,
                        QuantityReturned = d.QuantityReturned,
                        UnitCost = d.UnitCost
                    })
                    .ToList()
            })
            .ToList(),
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
