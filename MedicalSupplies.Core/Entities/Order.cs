using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class Order
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>Created -> Processing -> Dispatched -> Delivered, or Processing -> Cancelled.
    /// Transitions are validated in OrdersController, not left open-ended.</summary>
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Created;

    /// <summary>Independent of OrderStatus — e.g. Dispatched + PartiallyPaid is valid.</summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal AmountPaid { get; set; }

    public string? DeliveryLocation { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime? DispatchedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
