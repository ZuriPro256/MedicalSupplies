using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Security;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.Services;
using MedicalSupplies.Web.ViewModels.Admin.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPhoneNumberService _phoneNumberService;

    public CustomersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPhoneNumberService phoneNumberService)
    {
        _context = context;
        _userManager = userManager;
        _phoneNumberService = phoneNumberService;
    }

    [Authorize(Policy = Permissions.Customers.View)]
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(c =>
                (c.OrganizationName != null && EF.Functions.ILike(c.OrganizationName, $"%{term}%")) ||
                (c.FirstName != null && EF.Functions.ILike(c.FirstName, $"%{term}%")) ||
                (c.LastName != null && EF.Functions.ILike(c.LastName, $"%{term}%")) ||
                (c.Email != null && EF.Functions.ILike(c.Email, $"%{term}%")) ||
                (c.Phone != null && EF.Functions.ILike(c.Phone, $"%{term}%")));
        }

        var customers = await query
            .OrderBy(c => c.OrganizationName)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new CustomerListItemViewModel
            {
                CustomerId = c.CustomerId,
                OrganizationName = c.OrganizationName,
                CustomerName = (c.FirstName + " " + c.LastName).Trim(),
                Email = c.Email,
                Phone = c.Phone,
                CustomerType = c.CustomerType.ToString(),
                IsActive = c.IsActive
            })
            .ToListAsync();

        ViewBag.Search = search;
        return View(customers);
    }

    [Authorize(Policy = Permissions.Customers.View)]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Quotations)
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer is null)
            return NotFound();

        var model = new CustomerDetailsViewModel
        {
            CustomerId = customer.CustomerId,
            OrganizationName = customer.OrganizationName,
            CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
            Email = customer.Email,
            Phone = customer.Phone,
            CustomerType = customer.CustomerType.ToString(),
            Address = customer.Address,
            City = customer.City,
            IsActive = customer.IsActive,
            HasLogin = !string.IsNullOrWhiteSpace(customer.UserId),
            Quotations = customer.Quotations
                .OrderByDescending(q => q.RequestDate)
                .Select(q => new CustomerQuotationItemViewModel
                {
                    QuotationId = q.QuotationId,
                    QuotationNumber = q.QuotationNumber,
                    RequestDate = q.RequestDate,
                    Status = q.Status.ToString(),
                    TotalAmount = q.TotalAmount
                })
                .ToList(),
            Orders = customer.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new CustomerOrderItemViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount
                })
                .ToList()
        };

        return View(model);
    }

    [Authorize(Policy = Permissions.Customers.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer is null)
            return NotFound();

        var countryCode =
            _phoneNumberService.GetRegionCode(customer.Phone)
            ?? "UG";

        var phone =
            GetNationalPhoneNumber(customer.Phone);

        PopulateCountryOptions();

        return View(new CustomerEditViewModel
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            OrganizationName = customer.OrganizationName,
            CustomerType = customer.CustomerType,
            Email = customer.Email,
            CountryCode = countryCode,
            Phone = phone,
            Address = customer.Address,
            City = customer.City,
            IsActive = customer.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Customers.Edit)]
    public async Task<IActionResult> Edit(CustomerEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            PopulateCountryOptions();
            return View(vm);
        }

        var customer = await _context.Customers.FindAsync(vm.CustomerId);

        if (customer is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(vm.Phone))
        {
            if (!_phoneNumberService.TryNormalize(
                    vm.Phone,
                    vm.CountryCode,
                    out var normalizedPhone,
                    out var phoneError))
            {
                ModelState.AddModelError(
                    nameof(vm.Phone),
                    phoneError);

                PopulateCountryOptions();
                return View(vm);
            }

            customer.Phone = normalizedPhone;

            if (!string.IsNullOrWhiteSpace(customer.UserId))
            {
                var user =
                    await _userManager.FindByIdAsync(customer.UserId);

                if (user is not null)
                {
                    user.PhoneNumber = normalizedPhone;
                }
            }
        }
        else
        {
            customer.Phone = null;

            if (!string.IsNullOrWhiteSpace(customer.UserId))
            {
                var user =
                    await _userManager.FindByIdAsync(customer.UserId);

                if (user is not null)
                {
                    user.PhoneNumber = null;
                }
            }
        }

        customer.FirstName = vm.FirstName?.Trim();
        customer.LastName = vm.LastName?.Trim();
        customer.OrganizationName = vm.OrganizationName?.Trim();
        customer.CustomerType = vm.CustomerType;
        customer.Email = vm.Email?.Trim();
        customer.Address = vm.Address?.Trim();
        customer.City = vm.City?.Trim();
        customer.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Customer updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = customer.CustomerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Customers.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer is null)
                    throw new InvalidOperationException("CUSTOMER_NOT_FOUND");

                // Business history must never be deleted as a side effect
                // of deleting a customer.
                var hasOrders = await _context.Orders
                    .AnyAsync(o => o.CustomerId == id);

                var hasQuotations = await _context.Quotations
                    .AnyAsync(q => q.CustomerId == id);

                if (hasOrders || hasQuotations)
                    throw new InvalidOperationException("CUSTOMER_HAS_HISTORY");

                // If a login exists, verify it belongs to a Customer account
                // before deleting it. Never silently delete a staff account.
                ApplicationUser? loginUser = null;

                if (!string.IsNullOrWhiteSpace(customer.UserId))
                {
                    loginUser =
                        await _userManager.FindByIdAsync(customer.UserId);

                    if (loginUser is not null)
                    {
                        var isCustomerAccount =
                            await _userManager.IsInRoleAsync(
                                loginUser,
                                "Customer");

                        if (!isCustomerAccount)
                            throw new InvalidOperationException(
                                "LOGIN_NOT_CUSTOMER");
                    }
                }

                // Enquiries are configured with SetNull, so they remain
                // as historical enquiries without a customer link.
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                if (loginUser is not null)
                {
                    var deleteUserResult =
                        await _userManager.DeleteAsync(loginUser);

                    if (!deleteUserResult.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            deleteUserResult.Errors.Select(
                                e => e.Description));

                        throw new InvalidOperationException(
                            $"LOGIN_DELETE_FAILED:{errors}");
                    }
                }

                await transaction.CommitAsync();
            });

            TempData["Success"] = "Customer deleted successfully.";
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "CUSTOMER_NOT_FOUND")
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "CUSTOMER_HAS_HISTORY")
        {
            TempData["Error"] =
                "This customer cannot be permanently deleted because they have existing quotations or orders. Deactivate the customer instead.";
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "LOGIN_NOT_CUSTOMER")
        {
            TempData["Error"] =
                "This customer is linked to a non-customer login account. The customer was not deleted.";
        }
        catch (InvalidOperationException ex)
            when (ex.Message.StartsWith("LOGIN_DELETE_FAILED:"))
        {
            TempData["Error"] =
                "The customer could not be deleted because the linked login account could not be removed.";
        }
        catch (DbUpdateException)
        {
            // A new quotation/order could have been created concurrently
            // after the history check. The database FK protection wins.
            TempData["Error"] =
                "The customer could not be deleted because related business records exist. Deactivate the customer instead.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private void PopulateCountryOptions()
    {
        ViewBag.PhoneCountries =
            _phoneNumberService.GetCountries();
    }

    private string GetNationalPhoneNumber(string? e164Number)
    {
        if (string.IsNullOrWhiteSpace(e164Number))
            return string.Empty;

        try
        {
            var phoneUtil =
                PhoneNumbers.PhoneNumberUtil.GetInstance();

            var parsed =
                phoneUtil.Parse(e164Number, null);

            return phoneUtil.Format(
                parsed,
                PhoneNumbers.PhoneNumberFormat.NATIONAL);
        }
        catch (PhoneNumbers.NumberParseException)
        {
            return e164Number;
        }
    }
}
