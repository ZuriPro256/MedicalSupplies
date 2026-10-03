using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.Services;
using MedicalSupplies.Web.ViewModels.Catalogue;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Controllers;

/// <summary>
/// "Request a Quotation" flow: add products to a session cart from the
/// catalogue, review quantities, then submit contact details to turn the
/// cart into a real Quotation + QuotationDetails row for the admin side.
/// Works the same whether or not the visitor is signed in — a signed-in
/// customer's request is linked to their own account instead of being
/// matched by email.
/// </summary>
public class QuotationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IQuotationCartService _cart;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPhoneNumberService _phoneNumberService;

    public QuotationController(
        ApplicationDbContext context,
        IQuotationCartService cart,
        UserManager<ApplicationUser> userManager,
        IPhoneNumberService phoneNumberService)
    {
        _context = context;
        _cart = cart;
        _userManager = userManager;
        _phoneNumberService = phoneNumberService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(int productId, int quantity, string? returnUrl)
    {
        _cart.AddOrUpdate(
            HttpContext.Session,
            productId,
            quantity <= 0 ? 1 : quantity);

        TempData["Success"] = "Added to your quotation request.";

        if (!string.IsNullOrEmpty(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        _cart.AddOrUpdate(
            HttpContext.Session,
            productId,
            quantity);

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId)
    {
        _cart.Remove(
            HttpContext.Session,
            productId);

        return RedirectToAction(nameof(Cart));
    }

    public async Task<IActionResult> Cart()
    {
        var vm = new QuotationCartViewModel
        {
            Lines = await BuildLinesAsync()
        };

        return View(vm);
    }

    public new async Task<IActionResult> Request()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            var returnUrl =
                Url.Action(nameof(Request), "Quotation");

            return RedirectToAction(
                "Register",
                "Account",
                new { returnUrl });
        }

        var lines = await BuildLinesAsync();

        if (lines.Count == 0)
        {
            TempData["Error"] =
                "Your quotation list is empty — add some products first.";

            return RedirectToAction(
                "Index",
                "Products");
        }

        var vm = new QuotationRequestFormViewModel
        {
            Lines = lines
        };

        var signedInCustomer =
            await GetSignedInCustomerAsync();

        if (signedInCustomer is not null)
        {
            vm.ContactName =
                $"{signedInCustomer.FirstName} {signedInCustomer.LastName}"
                    .Trim();

            vm.OrganizationName =
                signedInCustomer.OrganizationName;

            vm.Email =
                signedInCustomer.Email ?? string.Empty;

            vm.CountryCode =
                _phoneNumberService.GetRegionCode(
                    signedInCustomer.Phone) ?? "UG";

            vm.Phone =
                GetNationalPhoneNumber(
                    signedInCustomer.Phone);

            vm.DeliveryLocation =
                signedInCustomer.Address;
        }

        PopulateCountryOptions();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public new async Task<IActionResult> Request(
        QuotationRequestFormViewModel vm)
    {
        vm.Lines = await BuildLinesAsync();

        if (vm.Lines.Count == 0)
        {
            TempData["Error"] =
                "Your quotation list is empty — add some products first.";

            return RedirectToAction(
                "Index",
                "Products");
        }

        if (!ModelState.IsValid)
        {
            PopulateCountryOptions();
            return View("Request", vm);
        }

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
            return View("Request", vm);
        }

        var customer =
            await GetSignedInCustomerAsync();

        if (customer is null)
        {
            customer =
                await _context.Customers.FirstOrDefaultAsync(
                    c =>
                        c.Email != null &&
                        EF.Functions.ILike(
                            c.Email,
                            vm.Email) &&
                        c.UserId == null);
        }

        if (customer is null)
        {
            var nameParts =
                vm.ContactName.Split(
                    ' ',
                    2,
                    StringSplitOptions.RemoveEmptyEntries);

            customer = new Customer
            {
                FirstName =
                    nameParts.ElementAtOrDefault(0),

                LastName =
                    nameParts.ElementAtOrDefault(1),

                OrganizationName =
                    vm.OrganizationName,

                Email =
                    vm.Email.Trim(),

                Phone =
                    normalizedPhone,

                CustomerType =
                    string.IsNullOrWhiteSpace(
                        vm.OrganizationName)
                        ? CustomerType.Individual
                        : CustomerType.Other
            };

            _context.Customers.Add(customer);
        }
        else
        {
            customer.Phone = normalizedPhone;
        }

        var quotation = new Quotation
        {
            QuotationNumber =
                $"QT-{DateTime.UtcNow:yyyyMMddHHmmss}",

            Customer = customer,

            DeliveryLocation =
                vm.DeliveryLocation,

            CustomerNotes =
                vm.CustomerNotes,

            Status =
                QuotationStatus.Pending
        };

        foreach (var line in vm.Lines)
        {
            quotation.Details.Add(
                new QuotationDetail
                {
                    ProductId =
                        line.ProductId,

                    Quantity =
                        line.Quantity
                });
        }

        _context.Quotations.Add(quotation);

        await _context.SaveChangesAsync();

        _cart.Clear(HttpContext.Session);

        return RedirectToAction(
            nameof(Confirmation),
            new
            {
                quotationNumber =
                    quotation.QuotationNumber
            });
    }

    public IActionResult Confirmation(
        string quotationNumber)
    {
        ViewBag.QuotationNumber =
            quotationNumber;

        return View();
    }

    private async Task<Customer?> GetSignedInCustomerAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        var userId =
            _userManager.GetUserId(User);

        if (userId is null)
            return null;

        return await _context.Customers
            .FirstOrDefaultAsync(
                c => c.UserId == userId);
    }

    private void PopulateCountryOptions()
    {
        ViewBag.PhoneCountries =
            _phoneNumberService.GetCountries();
    }

    private string GetNationalPhoneNumber(
        string? e164Number)
    {
        if (string.IsNullOrWhiteSpace(e164Number))
            return string.Empty;

        try
        {
            var phoneUtil =
                PhoneNumbers.PhoneNumberUtil.GetInstance();

            var parsed =
                phoneUtil.Parse(
                    e164Number,
                    null);

            return phoneUtil.Format(
                parsed,
                PhoneNumbers.PhoneNumberFormat.NATIONAL);
        }
        catch (PhoneNumbers.NumberParseException)
        {
            return e164Number;
        }
    }

    private async Task<List<QuotationCartLineViewModel>>
        BuildLinesAsync()
    {
        var cart =
            _cart.GetCart(HttpContext.Session);

        if (cart.Count == 0)
            return new List<QuotationCartLineViewModel>();

        var productIds =
            cart.Keys.ToList();

        var products =
            await _context.Products
                .Where(
                    p => productIds.Contains(
                        p.ProductId))
                .ToListAsync();

        return products
            .Select(
                p => new QuotationCartLineViewModel
                {
                    ProductId =
                        p.ProductId,

                    ProductName =
                        p.ProductName,

                    PackSize =
                        p.PackSize,

                    Quantity =
                        cart[p.ProductId]
                })
            .ToList();
    }
}
