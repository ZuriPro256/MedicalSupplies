namespace MedicalSupplies.Web.ViewModels.Admin;

public class AdminNotificationCountsViewModel
{
    public int Inquiries { get; set; }
    public int Quotations { get; set; }
    public int Orders { get; set; }
    public int AccountRecovery { get; set; }
    public int PurchaseOrders { get; set; }

    public bool CanSeeInquiries { get; set; }
    public bool CanSeeQuotations { get; set; }
    public bool CanSeeOrders { get; set; }
    public bool CanSeeAccountRecovery { get; set; }
    public bool CanSeePurchaseOrders { get; set; }
}
