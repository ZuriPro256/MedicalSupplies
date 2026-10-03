namespace MedicalSupplies.Core.Security;

public static class Permissions
{
    public static class Customers
    {
        public const string View = "Customers.View";
        public const string Create = "Customers.Create";
        public const string Edit = "Customers.Edit";
        public const string Delete = "Customers.Delete";
    }

    public static class Quotations
    {
        public const string View = "Quotations.View";
        public const string Create = "Quotations.Create";
        public const string Edit = "Quotations.Edit";
        public const string Send = "Quotations.Send";
        public const string Accept = "Quotations.Accept";
        public const string Reject = "Quotations.Reject";
    }

    public static class Offers
    {
        public const string View = "Offers.View";
        public const string Create = "Offers.Create";
        public const string Edit = "Offers.Edit";
        public const string Send = "Offers.Send";
        public const string Accept = "Offers.Accept";
        public const string Reject = "Offers.Reject";
    }

    public static class Orders
    {
        public const string View = "Orders.View";
        public const string Create = "Orders.Create";
        public const string Edit = "Orders.Edit";
        public const string Process = "Orders.Process";
        public const string Dispatch = "Orders.Dispatch";
        public const string Deliver = "Orders.Deliver";
    }

    public static class Products
    {
        public const string View = "Products.View";
        public const string Create = "Products.Create";
        public const string Edit = "Products.Edit";
    }

    public static class Inventory
    {
        public const string View = "Inventory.View";
        public const string Edit = "Inventory.Edit";
    }

    public static class Suppliers
    {
        public const string View = "Suppliers.View";
        public const string Create = "Suppliers.Create";
        public const string Edit = "Suppliers.Edit";
    }

    public static class PurchaseOrders
    {
        public const string View = "PurchaseOrders.View";
        public const string Create = "PurchaseOrders.Create";
        public const string Edit = "PurchaseOrders.Edit";
        public const string Receive = "PurchaseOrders.Receive";
    }

    public static class AccountRecovery
    {
        public const string View = "AccountRecovery.View";
        public const string Review = "AccountRecovery.Review";
    }

    public static class Staff
    {
        public const string View = "Staff.View";
        public const string Create = "Staff.Create";
        public const string Edit = "Staff.Edit";
        public const string ManagePermissions = "Staff.ManagePermissions";
    }
}