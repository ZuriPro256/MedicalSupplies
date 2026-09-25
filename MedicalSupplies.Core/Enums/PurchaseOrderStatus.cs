namespace MedicalSupplies.Core.Enums;

public enum PurchaseOrderStatus
{
    /// <summary>Being built — lines can still be added/edited/removed.</summary>
    Draft,

    /// <summary>Submitted for approval; lines are locked.</summary>
    Submitted,

    /// <summary>Approved — goods can now be received against this PO.</summary>
    Approved,

    /// <summary>Some, but not all, ordered quantity has been received.</summary>
    PartiallyReceived,

    /// <summary>Every line's ordered quantity has been received in full.</summary>
    Received,

    Cancelled
}
