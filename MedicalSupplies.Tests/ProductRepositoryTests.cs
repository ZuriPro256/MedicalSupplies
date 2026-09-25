using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MedicalSupplies.Core.Entities;

namespace MedicalSupplies.Tests;

public class ProductRepositoryTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Then_GetAllAsync_ReturnsTheProduct()
    {
        await using var context = CreateInMemoryContext();
        var repository = new GenericRepository<Product>(context);

        var category = new Category { CategoryName = "Diagnostic Equipment" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        await repository.AddAsync(new Product
        {
            ProductCode = "GLU-001",
            ProductName = "ON CALL PLUS Glucose Test Strips",
            CategoryId = category.CategoryId
        });
        await context.SaveChangesAsync();

        var products = await repository.GetAllAsync();

        Assert.Single(products);
        Assert.Equal("GLU-001", products[0].ProductCode);
    }
}
