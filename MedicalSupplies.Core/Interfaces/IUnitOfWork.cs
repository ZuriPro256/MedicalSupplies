using MedicalSupplies.Core.Entities;

namespace MedicalSupplies.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Product> Products { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Brand> Brands { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<Quotation> Quotations { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<InventoryBatch> InventoryBatches { get; }
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<Enquiry> Enquiries { get; }

    Task<int> CompleteAsync();
}
