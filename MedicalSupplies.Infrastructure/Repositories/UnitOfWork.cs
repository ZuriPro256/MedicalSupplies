using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Interfaces;
using MedicalSupplies.Infrastructure.Data;

namespace MedicalSupplies.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IGenericRepository<Product>? _products;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Brand>? _brands;
    private IGenericRepository<Customer>? _customers;
    private IGenericRepository<Quotation>? _quotations;
    private IGenericRepository<Order>? _orders;
    private IGenericRepository<InventoryBatch>? _inventoryBatches;
    private IGenericRepository<Supplier>? _suppliers;
    private IGenericRepository<Enquiry>? _enquiries;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public IGenericRepository<Product> Products => _products ??= new GenericRepository<Product>(_context);
    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IGenericRepository<Brand> Brands => _brands ??= new GenericRepository<Brand>(_context);
    public IGenericRepository<Customer> Customers => _customers ??= new GenericRepository<Customer>(_context);
    public IGenericRepository<Quotation> Quotations => _quotations ??= new GenericRepository<Quotation>(_context);
    public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);
    public IGenericRepository<InventoryBatch> InventoryBatches => _inventoryBatches ??= new GenericRepository<InventoryBatch>(_context);
    public IGenericRepository<Supplier> Suppliers => _suppliers ??= new GenericRepository<Supplier>(_context);
    public IGenericRepository<Enquiry> Enquiries => _enquiries ??= new GenericRepository<Enquiry>(_context);

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
