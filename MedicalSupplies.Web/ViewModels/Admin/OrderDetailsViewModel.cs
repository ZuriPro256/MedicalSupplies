using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class OrderDetailsViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? DeliveryLocation { get; set; }

    public List<OrderLineViewModel> Lines { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal AmountPaid { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime? DispatchedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }

    public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new();

    public List<OrderStatus> AvailableNextStatuses { get; set; } = new();
}

public class OrderLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderStatusHistoryViewModel
{
    public OrderStatus Status { get; set; }
    public DateTime ChangedDate { get; set; }
    public string? ChangedBy { get; set; }
    public string? Notes { get; set; }
}
