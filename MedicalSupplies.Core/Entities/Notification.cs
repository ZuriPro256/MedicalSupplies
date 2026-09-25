namespace MedicalSupplies.Core.Entities;

public class Notification
{
    public int NotificationId { get; set; }

    /// <summary>AspNetUsers.Id of the recipient; null for a broadcast/admin-wide notification.</summary>
    public string? UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>e.g. "LowStock", "ExpiryAlert", "NewQuotation", "NewOrder".</summary>
    public string NotificationType { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
