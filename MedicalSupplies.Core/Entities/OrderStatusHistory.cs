using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

/// <summary>
/// One row per status change on an Order — gives the audit trail a
/// medical-supplies business needs ("23 Sep 10:15 — Processing, by ...").
/// Written by OrdersController every time OrderStatus changes, including
/// the initial Created row at the moment a quotation is converted.
/// </summary>
public class OrderStatusHistory
{
    public int OrderStatusHistoryId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

    /// <summary>AspNetUsers.Id (or display name) of who made the change; null until Identity UI exists.</summary>
    public string? ChangedBy { get; set; }

    public string? Notes { get; set; }
}
