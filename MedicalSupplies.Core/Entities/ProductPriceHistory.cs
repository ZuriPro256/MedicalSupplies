using System;

namespace MedicalSupplies.Core.Entities;

public class ProductPriceHistory
{
    public int ProductPriceHistoryId { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string PriceType { get; set; } = string.Empty;

    public decimal? PreviousAmount { get; set; }
    public decimal? NewAmount { get; set; }

    public string Currency { get; set; } = "UGX";

    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

    public string? ChangedBy { get; set; }

    public string? Reason { get; set; }
}
