namespace MedicalSupplies.Core.Entities;

public class Notification
{
    public int NotificationId { get; set; }

    /// <summary>
    /// AspNetUsers.Id of the recipient; null for a broadcast/admin-wide notification.
    /// </summary>
    public string? UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// e.g. "QuotationSent", "RevisedOfferSent", "OrderStatusChanged",
    /// "AccountRecoveryApproved", "AccountRecoveryRejected", "InquiryResolved".
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>
    /// Optional local URL to the related customer page.
    /// </summary>
    public string? Url { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
