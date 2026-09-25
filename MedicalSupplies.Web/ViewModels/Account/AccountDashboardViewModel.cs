using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Account;

public class AccountDashboardViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public int QuotationCount { get; set; }
    public int OrderCount { get; set; }
    public int PendingQuotationCount { get; set; }
    public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
}

public class RecentOrderViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus OrderStatus { get; set; }
}
