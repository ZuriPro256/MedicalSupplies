namespace MedicalSupplies.Core.Enums;

public enum QuotationStatus
{
    /// <summary>Customer has requested a quotation; admin hasn't started pricing it.</summary>
    Pending,

    /// <summary>Admin has set/edited prices and saved, but not yet sent to the customer.</summary>
    Pricing,

    /// <summary>Priced quotation has been sent to the customer.</summary>
    Sent,

    /// <summary>Customer accepted the quotation. Does not create an Order by itself —
    /// see the deliberate "Convert to Order" action.</summary>
    Accepted,

    Rejected,
    Expired,

    /// <summary>An Order has been created from this quotation.</summary>
    ConvertedToOrder
}
