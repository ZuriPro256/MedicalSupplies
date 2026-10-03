using System.Diagnostics;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Catalogue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var featured = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Batches)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedDate)
            .Take(6)
            .ToListAsync();

        var vm = featured.Select(p => new CatalogueProductCardViewModel
        {
            ProductId = p.ProductId,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,
            PrimaryImageUrl = p.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl,
            CategoryName = p.Category.CategoryName,
            BrandName = p.Brand?.BrandName,
            PackSize = p.PackSize,
            InStock = !p.RequiresBatchTracking || p.Batches.Any(b =>
                b.Status == Core.Enums.BatchStatus.Active &&
                b.QuantityAvailable > 0 &&
                (!b.ExpiryDate.HasValue ||
                 b.ExpiryDate.Value >= DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3))))
        }).ToList();

        return View(vm);
    }

    public IActionResult Error()
    {
        return View(new { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
