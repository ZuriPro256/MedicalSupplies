namespace MedicalSupplies.Core.Security;

public static class RolePermissions
{
    public static IReadOnlyList<string> ForRole(string role)
    {
        return role switch
        {
            "SuperAdmin" => All,

            "Admin" => new[]
            {
                Permissions.Customers.View,
                Permissions.Customers.Create,
                Permissions.Customers.Edit,
                Permissions.Customers.Delete,

                Permissions.Quotations.View,
                Permissions.Quotations.Create,
                Permissions.Quotations.Edit,
                Permissions.Quotations.Send,
                Permissions.Quotations.Accept,
                Permissions.Quotations.Reject,

                Permissions.Offers.View,
                Permissions.Offers.Create,
                Permissions.Offers.Edit,
                Permissions.Offers.Send,
                Permissions.Offers.Accept,
                Permissions.Offers.Reject,

                Permissions.Orders.View,
                Permissions.Orders.Create,
                Permissions.Orders.Edit,
                Permissions.Orders.Process,
                Permissions.Orders.Dispatch,
                Permissions.Orders.Deliver,

                Permissions.Products.View,
                Permissions.Products.Create,
                Permissions.Products.Edit,

                Permissions.Inventory.View,
                Permissions.Inventory.Edit,

                Permissions.Suppliers.View,
                Permissions.Suppliers.Create,
                Permissions.Suppliers.Edit,

                Permissions.PurchaseOrders.View,
                Permissions.PurchaseOrders.Create,
                Permissions.PurchaseOrders.Edit,
                Permissions.PurchaseOrders.Receive,

                Permissions.AccountRecovery.View,
                Permissions.AccountRecovery.Review
            },

            "Sales" => new[]
            {
                Permissions.Customers.View,
                Permissions.Customers.Create,
                Permissions.Customers.Edit,

                Permissions.Quotations.View,
                Permissions.Quotations.Create,
                Permissions.Quotations.Edit,
                Permissions.Quotations.Send,
                Permissions.Quotations.Accept,
                Permissions.Quotations.Reject,

                Permissions.Offers.View,
                Permissions.Offers.Create,
                Permissions.Offers.Edit,
                Permissions.Offers.Send,
                Permissions.Offers.Accept,
                Permissions.Offers.Reject,

                Permissions.Orders.View,
                Permissions.Orders.Create
            },

            "InventoryManager" => new[]
            {
                Permissions.Products.View,
                Permissions.Products.Create,
                Permissions.Products.Edit,

                Permissions.Inventory.View,
                Permissions.Inventory.Edit,

                Permissions.Suppliers.View,
                Permissions.Suppliers.Create,
                Permissions.Suppliers.Edit,

                Permissions.PurchaseOrders.View,
                Permissions.PurchaseOrders.Create,
                Permissions.PurchaseOrders.Edit,
                Permissions.PurchaseOrders.Receive,

                Permissions.Orders.View
            },

            "Customer" => Array.Empty<string>(),

            _ => Array.Empty<string>()
        };
    }

    public static IReadOnlyList<string> All =>
    [
        Permissions.Customers.View,
        Permissions.Customers.Create,
        Permissions.Customers.Edit,
        Permissions.Customers.Delete,

        Permissions.Quotations.View,
        Permissions.Quotations.Create,
        Permissions.Quotations.Edit,
        Permissions.Quotations.Send,
        Permissions.Quotations.Accept,
        Permissions.Quotations.Reject,

        Permissions.Offers.View,
        Permissions.Offers.Create,
        Permissions.Offers.Edit,
        Permissions.Offers.Send,
        Permissions.Offers.Accept,
        Permissions.Offers.Reject,

        Permissions.Orders.View,
        Permissions.Orders.Create,
        Permissions.Orders.Edit,
        Permissions.Orders.Process,
        Permissions.Orders.Dispatch,
        Permissions.Orders.Deliver,

        Permissions.Products.View,
        Permissions.Products.Create,
        Permissions.Products.Edit,

        Permissions.Inventory.View,
        Permissions.Inventory.Edit,

        Permissions.Suppliers.View,
        Permissions.Suppliers.Create,
        Permissions.Suppliers.Edit,

        Permissions.PurchaseOrders.View,
        Permissions.PurchaseOrders.Create,
        Permissions.PurchaseOrders.Edit,
        Permissions.PurchaseOrders.Receive,

        Permissions.AccountRecovery.View,
        Permissions.AccountRecovery.Review,

        Permissions.Staff.View,
        Permissions.Staff.Create,
        Permissions.Staff.Edit,
        Permissions.Staff.ManagePermissions
    ];
}