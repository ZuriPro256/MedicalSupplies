using MedicalSupplies.Core.Entities;
using MedicalSupplies.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<InventoryBatch> InventoryBatches => Set<InventoryBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AccountRecoveryRequest> AccountRecoveryRequests => Set<AccountRecoveryRequest>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationDetail> QuotationDetails => Set<QuotationDetail>();
    public DbSet<QuotationOffer> QuotationOffers => Set<QuotationOffer>();
    public DbSet<QuotationOfferDetail> QuotationOfferDetails => Set<QuotationOfferDetail>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PurchaseOrderStatusHistory> PurchaseOrderStatusHistories
        => Set<PurchaseOrderStatusHistory>();

    public DbSet<PurchaseOrderReturn> PurchaseOrderReturns
        => Set<PurchaseOrderReturn>();

    public DbSet<PurchaseOrderReturnDetail> PurchaseOrderReturnDetails
        => Set<PurchaseOrderReturnDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly
        // (see Data/Configurations).
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
