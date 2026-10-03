using MedicalSupplies.Core.Security;

namespace MedicalSupplies.Web.Security;

public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new("Customers", "View customers", Permissions.Customers.View),
        new("Customers", "Create customers", Permissions.Customers.Create),
        new("Customers", "Edit customers", Permissions.Customers.Edit),
        new("Customers", "Delete customers", Permissions.Customers.Delete),

        new("Quotations", "View quotations", Permissions.Quotations.View),
        new("Quotations", "Create quotations", Permissions.Quotations.Create),
        new("Quotations", "Edit quotations", Permissions.Quotations.Edit),
        new("Quotations", "Send quotations", Permissions.Quotations.Send),
        new("Quotations", "Accept quotations", Permissions.Quotations.Accept),
        new("Quotations", "Reject quotations", Permissions.Quotations.Reject),

        new("Offers", "View offers", Permissions.Offers.View),
        new("Offers", "Create offers", Permissions.Offers.Create),
        new("Offers", "Edit offers", Permissions.Offers.Edit),
        new("Offers", "Send offers", Permissions.Offers.Send),
        new("Offers", "Accept offers", Permissions.Offers.Accept),
        new("Offers", "Reject offers", Permissions.Offers.Reject),

        new("Orders", "View orders", Permissions.Orders.View),
        new("Orders", "Create orders", Permissions.Orders.Create),
        new("Orders", "Edit orders", Permissions.Orders.Edit),
        new("Orders", "Process orders", Permissions.Orders.Process),
        new("Orders", "Dispatch orders", Permissions.Orders.Dispatch),
        new("Orders", "Deliver orders", Permissions.Orders.Deliver),

        new("Products", "View products", Permissions.Products.View),
        new("Products", "Create products", Permissions.Products.Create),
        new("Products", "Edit products", Permissions.Products.Edit),

        new("Inventory", "View inventory", Permissions.Inventory.View),
        new("Inventory", "Edit inventory", Permissions.Inventory.Edit),

        new("Suppliers", "View suppliers", Permissions.Suppliers.View),
        new("Suppliers", "Create suppliers", Permissions.Suppliers.Create),
        new("Suppliers", "Edit suppliers", Permissions.Suppliers.Edit),

        new("Purchase Orders", "View purchase orders", Permissions.PurchaseOrders.View),
        new("Purchase Orders", "Create purchase orders", Permissions.PurchaseOrders.Create),
        new("Purchase Orders", "Edit purchase orders", Permissions.PurchaseOrders.Edit),
        new("Purchase Orders", "Receive purchase orders", Permissions.PurchaseOrders.Receive),

        new("Account Recovery", "View account recovery requests", Permissions.AccountRecovery.View),
        new("Account Recovery", "Review account recovery requests", Permissions.AccountRecovery.Review),

        new("Staff", "View staff", Permissions.Staff.View),
        new("Staff", "Create staff", Permissions.Staff.Create),
        new("Staff", "Edit staff", Permissions.Staff.Edit),
        new("Staff", "Manage permissions", Permissions.Staff.ManagePermissions)
    ];
}

public sealed record PermissionDefinition(
    string Category,
    string Description,
    string Permission);