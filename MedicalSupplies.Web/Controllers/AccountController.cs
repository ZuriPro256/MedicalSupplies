using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Controllers;

/// <summary>
/// Registration/login (matches ASP.NET Core Identity's default cookie
/// paths, /Account/Login etc. — no extra configuration needed) plus the
/// customer-facing account area: dashboard, My Quotations, My Orders,
/// profile. Everything below Register/Login requires a signed-in customer,
/// and every lookup is filtered by *that* customer's own UserId — never
/// by an id alone — so requesting someone else's quotation or order
/// number returns 404, not their data.
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    public IActionResult Register(string? returnUrl) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            PhoneNumber = vm.Phone,
            FullName = vm.FullName
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        // A guest may have already requested a quotation with this email
        // before ever registering — link that existing Customer record
        // rather than creating a duplicate one.
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email != null && EF.Functions.ILike(c.Email, vm.Email) && c.UserId == null);
        var nameParts = vm.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (customer is null)
        {
            customer = new Customer
            {
                UserId = user.Id,
                FirstName = nameParts.ElementAtOrDefault(0),
                LastName = nameParts.ElementAtOrDefault(1),
                OrganizationName = vm.OrganizationName,
                CustomerType = string.IsNullOrWhiteSpace(vm.OrganizationName) ? CustomerType.Individual : CustomerType.Other,
                Email = vm.Email,
                Phone = vm.Phone,
                Address = vm.Address
            };
            _context.Customers.Add(customer);
        }
        else
        {
            customer.UserId = user.Id;
            customer.FirstName ??= nameParts.ElementAtOrDefault(0);
            customer.LastName ??= nameParts.ElementAtOrDefault(1);
            customer.OrganizationName ??= vm.OrganizationName;
            customer.Address ??= vm.Address;
        }

        await _context.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToLocalOrDashboard(vm.ReturnUrl);
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        return RedirectToLocalOrDashboard(vm.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    public async Task<IActionResult> Dashboard()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        var quotations = await _context.Quotations
            .Where(q => q.CustomerId == customer.CustomerId)
            .ToListAsync();

        var recentOrders = await _context.Orders
            .Where(o => o.CustomerId == customer.CustomerId)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new RecentOrderViewModel
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderStatus = o.OrderStatus
            })
            .ToListAsync();

        var vm = new AccountDashboardViewModel
        {
            DisplayName = customer.OrganizationName ?? $"{customer.FirstName} {customer.LastName}".Trim(),
            QuotationCount = quotations.Count,
            OrderCount = await _context.Orders.CountAsync(o => o.CustomerId == customer.CustomerId),
            PendingQuotationCount = quotations.Count(q => q.Status is QuotationStatus.Pending or QuotationStatus.Pricing or QuotationStatus.Sent),
            RecentOrders = recentOrders
        };
        return View(vm);
    }

    public async Task<IActionResult> Quotations()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        var quotations = await _context.Quotations
            .Where(q => q.CustomerId == customer.CustomerId)
            .OrderByDescending(q => q.RequestDate)
            .Select(q => new MyQuotationListItemViewModel
            {
                QuotationId = q.QuotationId,
                QuotationNumber = q.QuotationNumber,
                RequestDate = q.RequestDate,
                TotalAmount = q.TotalAmount,
                Status = q.Status
            })
            .ToListAsync();

        return View(quotations);
    }

    public async Task<IActionResult> QuotationDetails(int id)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        // Ownership check happens in the query itself, not after the fact —
        // a quotation belonging to another customer simply doesn't match
        // and comes back as 404, never as a 403 that confirms it exists.
        var quotation = await _context.Quotations
            .Include(q => q.Details).ThenInclude(d => d.Product)
            .Include(q => q.Orders)
            .FirstOrDefaultAsync(q => q.QuotationId == id && q.CustomerId == customer.CustomerId);

        if (quotation is null) return NotFound();

        var convertedOrder = quotation.Orders.FirstOrDefault();

        var vm = new MyQuotationDetailsViewModel
        {
            QuotationId = quotation.QuotationId,
            QuotationNumber = quotation.QuotationNumber,
            RequestDate = quotation.RequestDate,
            Status = quotation.Status,
            DeliveryLocation = quotation.DeliveryLocation,
            ValidUntil = quotation.ValidUntil,
            Lines = quotation.Details.Select(d => new MyQuotationLineViewModel
            {
                ProductName = d.Product.ProductName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                LineTotal = d.TotalPrice
            }).ToList(),
            SubTotal = quotation.Details.Sum(d => d.TotalPrice ?? 0),
            DiscountAmount = quotation.DiscountAmount,
            TotalAmount = quotation.TotalAmount,
            OrderId = convertedOrder?.OrderId,
            OrderNumber = convertedOrder?.OrderNumber
        };
        return View(vm);
    }

    public async Task<IActionResult> Orders()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        var orders = await _context.Orders
            .Where(o => o.CustomerId == customer.CustomerId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new MyOrderListItemViewModel
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                DeliveredDate = o.DeliveredDate
            })
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> OrderDetails(int id)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        // Same pattern as QuotationDetails: ownership is part of the query,
        // not a check bolted on afterwards. order.Customer.UserId is never
        // trusted from a route value — only from the signed-in principal.
        var order = await _context.Orders
            .Include(o => o.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customer.CustomerId);

        if (order is null) return NotFound();

        var vm = new MyOrderDetailsViewModel
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            DeliveryLocation = order.DeliveryLocation,
            Lines = order.Details.Select(d => new MyOrderLineViewModel
            {
                ProductName = d.Product.ProductName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TotalPrice = d.TotalPrice
            }).ToList(),
            Subtotal = order.Subtotal,
            Discount = order.Discount,
            TotalAmount = order.TotalAmount,
            OrderStatus = order.OrderStatus,
            PaymentStatus = order.PaymentStatus,
            AmountPaid = order.AmountPaid,
            DispatchedDate = order.DispatchedDate,
            DeliveredDate = order.DeliveredDate
        };
        return View(vm);
    }

    public async Task<IActionResult> Profile()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        return View(new ProfileViewModel
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            OrganizationName = customer.OrganizationName,
            Email = customer.Email ?? string.Empty,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel vm)
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer is null) return RedirectToAction(nameof(Login));

        vm.Email = customer.Email ?? string.Empty; // Email changes aren't handled here — Identity email change needs its own confirmation flow.
        if (!ModelState.IsValid) return View(vm);

        customer.FirstName = vm.FirstName;
        customer.LastName = vm.LastName;
        customer.OrganizationName = vm.OrganizationName;
        customer.Phone = vm.Phone;
        customer.Address = vm.Address;
        customer.City = vm.City;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task<Customer?> GetCurrentCustomerAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return null;
        return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private IActionResult RedirectToLocalOrDashboard(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (User.IsInRole("SuperAdmin") ||
            User.IsInRole("Admin") ||
            User.IsInRole("Sales") ||
            User.IsInRole("InventoryManager"))
        {
            return RedirectToAction(
                "Index",
                "Dashboard",
                new { area = "Admin" });
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
