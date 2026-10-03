using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Services;

/// <summary>
/// Stock is never touched when a quotation is created or even when it's
/// converted to an order — only here, when an order is confirmed for
/// fulfilment (moved to Processing), and reversed if that order is later
/// cancelled. Keeps abandoned quotations from ever corrupting stock figures.
///
/// StockMovements is the single authoritative record of which batch(es)
/// fulfilled which order line — OrderDetail carries no BatchId of its own,
/// so there's exactly one place this information can live and disagree
/// with itself.
/// </summary>
public interface IOrderFulfillmentService
{
    Task<StockAllocationResult> DeductStockForOrderAsync(Order order, string? changedBy);
    Task ReverseStockForOrderAsync(Order order, string? changedBy);
}

public record StockAllocationResult(bool Success, List<string> InsufficientProducts);

public class OrderFulfillmentService : IOrderFulfillmentService
{
    private readonly ApplicationDbContext _context;

    public OrderFulfillmentService(ApplicationDbContext context) => _context = context;

    public async Task<StockAllocationResult> DeductStockForOrderAsync(Order order, string? changedBy)
    {
        var insufficient = new List<string>();

        // Load the products first so we know which order lines actually require
        // batch-level stock tracking.
        var productIds = order.Details.Select(d => d.ProductId).Distinct().ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId);

        var trackedProductIds = order.Details
            .Where(d => products[d.ProductId].RequiresBatchTracking)
            .Select(d => d.ProductId)
            .Distinct()
            .ToList();

        if (trackedProductIds.Count == 0)
            return new StockAllocationResult(true, insufficient);

        // The ChangeStatus transaction is already open when this method is called.
        // FOR UPDATE locks the actual inventory rows until that transaction commits
        // or rolls back. This prevents two concurrent orders from both consuming
        // the same QuantityAvailable value.
        //
        // Batch IDs are used as the lock order so concurrent transactions acquire
        // locks consistently, reducing the chance of deadlocks.
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        var batchList = await _context.InventoryBatches
            .FromSqlInterpolated($"""
                SELECT *
                FROM "InventoryBatches"
                WHERE "ProductId" = ANY({trackedProductIds.ToArray()})
                  AND "Status" = {BatchStatus.Active.ToString()}
                  AND "QuantityAvailable" > 0
                  AND ("ExpiryDate" IS NULL OR "ExpiryDate" >= {today})
                ORDER BY "BatchId"
                FOR UPDATE
                """)
            .ToListAsync();

        // Re-sort after locking into FEFO order. The database lock order remains
        // BatchId ascending; allocation order remains earliest expiry first.
        batchList = batchList
            .OrderBy(b => b.ExpiryDate ?? DateOnly.MaxValue)
            .ThenBy(b => b.BatchId)
            .ToList();

        var batchesByProduct = batchList
            .GroupBy(b => b.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Check total available stock first — an order should never end up
        // partially deducted. Nothing is written until every tracked line clears.
        foreach (var detail in order.Details)
        {
            var product = products[detail.ProductId];
            if (!product.RequiresBatchTracking) continue;

            var available = batchesByProduct.TryGetValue(detail.ProductId, out var batches)
                ? batches.Sum(b => b.QuantityAvailable)
                : 0;

            if (available < detail.Quantity)
            {
                insufficient.Add($"{product.ProductName} (need {detail.Quantity}, have {available})");
            }
        }

        if (insufficient.Count > 0)
        {
            return new StockAllocationResult(false, insufficient);
        }

        // Sufficient everywhere — now actually allocate, earliest-expiry-first.
        // One StockMovement per batch touched; that set of rows is the only
        // record of which batches fulfilled this line.
        foreach (var detail in order.Details)
        {
            var product = products[detail.ProductId];
            if (!product.RequiresBatchTracking) continue;

            var remaining = detail.Quantity;
            var batches = batchesByProduct[detail.ProductId];

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                var take = Math.Min(remaining, batch.QuantityAvailable);
                if (take <= 0) continue;

                batch.QuantityAvailable -= take;
                remaining -= take;

                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = detail.ProductId,
                    BatchId = batch.BatchId,
                    MovementType = StockMovementType.OrderProcessing,
                    QuantityChange = -take,
                    OrderId = order.OrderId,
                    OrderDetailId = detail.OrderDetailId,
                    Notes = $"Allocated to order {order.OrderNumber}.",
                    CreatedBy = changedBy
                });
            }
        }

        return new StockAllocationResult(true, insufficient);
    }

    public async Task ReverseStockForOrderAsync(Order order, string? changedBy)
    {
        var processingMovements = await _context.StockMovements
            .Where(m => m.OrderId == order.OrderId && m.MovementType == StockMovementType.OrderProcessing)
            .ToListAsync();

        if (processingMovements.Count == 0) return; // Nothing was ever deducted (order was cancelled before Processing).

        var batchIds = processingMovements.Where(m => m.BatchId.HasValue).Select(m => m.BatchId!.Value).Distinct().ToList();
        var batches = await _context.InventoryBatches
            .FromSqlInterpolated($"""
                SELECT *
                FROM "InventoryBatches"
                WHERE "BatchId" = ANY({batchIds.ToArray()})
                ORDER BY "BatchId"
                FOR UPDATE
                """)
            .ToDictionaryAsync(b => b.BatchId);

        // Each original OrderProcessing movement is left exactly as it was —
        // we never edit or delete it. Every reversal is a brand-new row that
        // points back at the movement it reverses, so the audit trail reads
        // "movement #108, OrderCancellation, +200, reverses #101" rather than
        // #101 silently changing.
        foreach (var movement in processingMovements)
        {
            var restoreQty = -movement.QuantityChange; // OrderProcessing movements are stored negative.
            if (movement.BatchId.HasValue && batches.TryGetValue(movement.BatchId.Value, out var batch))
            {
                batch.QuantityAvailable += restoreQty;
            }

            _context.StockMovements.Add(new StockMovement
            {
                ProductId = movement.ProductId,
                BatchId = movement.BatchId,
                MovementType = StockMovementType.OrderCancellation,
                QuantityChange = restoreQty,
                OrderId = order.OrderId,
                OrderDetailId = movement.OrderDetailId,
                ReversesStockMovementId = movement.StockMovementId,
                Notes = $"Reversed — order {order.OrderNumber} cancelled.",
                CreatedBy = changedBy
            });
        }
    }
}
