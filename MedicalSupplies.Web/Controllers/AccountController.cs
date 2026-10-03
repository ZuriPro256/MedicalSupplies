using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.Services;
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
/// and every lookup is filtered by that customer's own UserId — never
/// by an id alone — so requesting someone else's quotation or order
/// number returns 404, not their data.
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPhoneNumberService _phoneNumberService;
    private readonly ICustomerNotificationService _customerNotificationService;

    public AccountController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IPhoneNumberService phoneNumberService,
        ICustomerNotificationService customerNotificationService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _phoneNumberService = phoneNumberService;
        _customerNotificationService = customerNotificationService;
    }

    [AllowAnonymous]
    public IActionResult Register(string? returnUrl)
    {
        var vm = new RegisterViewModel
        {
            ReturnUrl = returnUrl
        };

        PopulateCountryOptions();

        return View(vm);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        string? normalizedPhone = null;

        if (ModelState.IsValid)
        {
            if (!_phoneNumberService.TryNormalize(
                    vm.Phone,
                    vm.CountryCode,
                    out normalizedPhone,
                    out var phoneError))
            {
                ModelState.AddModelError(
                    nameof(vm.Phone),
                    phoneError);
            }
        }

        if (!ModelState.IsValid)
        {
            PopulateCountryOptions();
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email.Trim(),
            Email = vm.Email.Trim(),
            PhoneNumber = normalizedPhone,
            FullName = vm.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(user, vm.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            PopulateCountryOptions();
            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        // A guest may have already requested a quotation with this email
        // before ever registering — link that existing Customer record
        // rather than creating a duplicate one.
        var email = vm.Email.Trim();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Email != null &&
                EF.Functions.ILike(c.Email, email) &&
                c.UserId == null);

        var nameParts = vm.FullName
            .Trim()
            .Split(
                ' ',
                2,
                StringSplitOptions.RemoveEmptyEntries);

        if (customer is null)
        {
            customer = new Customer
            {
                UserId = user.Id,
                FirstName = nameParts.ElementAtOrDefault(0),
                LastName = nameParts.ElementAtOrDefault(1),
                OrganizationName = string.IsNullOrWhiteSpace(vm.OrganizationName)
                    ? null
                    : vm.OrganizationName.Trim(),
                CustomerType = string.IsNullOrWhiteSpace(vm.OrganizationName)
                    ? CustomerType.Individual
                    : CustomerType.Other,
                Email = email,
                Phone = normalizedPhone,
                Address = string.IsNullOrWhiteSpace(vm.Address)
                    ? null
                    : vm.Address.Trim()
            };

            _context.Customers.Add(customer);
        }
        else
        {
            customer.UserId = user.Id;
            customer.FirstName ??= nameParts.ElementAtOrDefault(0);
            customer.LastName ??= nameParts.ElementAtOrDefault(1);
            customer.OrganizationName ??=
                string.IsNullOrWhiteSpace(vm.OrganizationName)
                    ? null
                    : vm.OrganizationName.Trim();
            customer.Address ??=
                string.IsNullOrWhiteSpace(vm.Address)
                    ? null
                    : vm.Address.Trim();

            // The newly registered account's verified contact number
            // becomes the canonical number for the existing Customer record.
            customer.Phone = normalizedPhone;
            customer.Email = email;
        }

        await _context.SaveChangesAsync();

        await _signInManager.SignInAsync(
            user,
            isPersistent: false);

        return RedirectToLocalOrDashboard(vm.ReturnUrl);
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl) =>
        View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        PopulateCountryOptions();

        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            PopulateCountryOptions();
            return View(vm);
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
            return View(vm);
        }

        var email = vm.Email.Trim();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.Email != null &&
                c.Phone != null &&
                EF.Functions.ILike(c.Email, email) &&
                c.Phone == normalizedPhone &&
                c.UserId != null);

        if (customer != null)
        {
            var request = new AccountRecoveryRequest
            {
                CustomerId = customer.CustomerId,
                RecoveryType = "RegisteredContact",
                SubmittedEmail = email,
                SubmittedPhone = normalizedPhone,
                Reason = vm.Reason?.Trim(),
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.AccountRecoveryRequests.Add(request);

            await _context.SaveChangesAsync();
        }

        TempData["Success"] =
            "If the details match a registered account, your recovery request has been submitted for review.";

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _signInManager.PasswordSignInAsync(
            vm.Email,
            vm.Password,
            vm.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

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
    public IActionResult AccessDenied() =>
        View();

    public async Task<IActionResult> Dashboard()
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

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
            DisplayName =
                customer.OrganizationName ??
                $"{customer.FirstName} {customer.LastName}".Trim(),

            QuotationCount = quotations.Count,

            OrderCount = await _context.Orders
                .CountAsync(o =>
                    o.CustomerId == customer.CustomerId),

            PendingQuotationCount = quotations.Count(q =>
                q.Status is
                    QuotationStatus.Pending or
                    QuotationStatus.Pricing or
                    QuotationStatus.Sent),

            RecentOrders = recentOrders
        };

        return View(vm);
    }

    public async Task<IActionResult> Quotations()
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptQuotation(int id)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var quotation = await _context.Quotations
            .FirstOrDefaultAsync(q =>
                q.QuotationId == id &&
                q.CustomerId == customer.CustomerId);

        if (quotation is null)
            return NotFound();

        if (quotation.Status != QuotationStatus.Sent)
        {
            TempData["Error"] =
                "Only a quotation that has been sent can be accepted.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id });
        }

        quotation.Status = QuotationStatus.Accepted;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Quotation accepted successfully.";

        return RedirectToAction(
            nameof(QuotationDetails),
            new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectQuotation(
        int id,
        QuotationRejectionReason? rejectionReason,
        decimal? customerExpectedPrice,
        string? customerRejectionComment)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var quotation = await _context.Quotations
            .FirstOrDefaultAsync(q =>
                q.QuotationId == id &&
                q.CustomerId == customer.CustomerId);

        if (quotation is null)
            return NotFound();

        if (quotation.Status != QuotationStatus.Sent)
        {
            TempData["Error"] =
                "Only a quotation that has been sent can be rejected.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id });
        }

        if (rejectionReason is null)
        {
            TempData["Error"] =
                "Please select a reason for rejecting the quotation.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id });
        }

        if (customerExpectedPrice is not null &&
            customerExpectedPrice < 0)
        {
            TempData["Error"] =
                "Expected price cannot be negative.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id });
        }

        quotation.Status = QuotationStatus.Rejected;
        quotation.RejectionReason = rejectionReason;
        quotation.CustomerExpectedPrice = customerExpectedPrice;
        quotation.CustomerRejectionComment =
            string.IsNullOrWhiteSpace(customerRejectionComment)
                ? null
                : customerRejectionComment.Trim();
        quotation.RejectedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Quotation rejected. Your feedback has been sent to the supplier.";

        return RedirectToAction(
            nameof(QuotationDetails),
            new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptOffer(int id)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var offer = await _context.QuotationOffers
            .Include(o => o.Quotation)
            .FirstOrDefaultAsync(o =>
                o.QuotationOfferId == id &&
                o.Quotation.CustomerId == customer.CustomerId);

        if (offer is null)
            return NotFound();

        if (offer.Status != QuotationOfferStatus.Sent)
        {
            TempData["Error"] =
                "Only a sent revised offer can be accepted.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id = offer.QuotationId });
        }

        var otherActiveOffers = await _context.QuotationOffers
            .Where(o =>
                o.QuotationId == offer.QuotationId &&
                o.QuotationOfferId != offer.QuotationOfferId &&
                (o.Status == QuotationOfferStatus.Draft ||
                 o.Status == QuotationOfferStatus.Sent))
            .ToListAsync();

        foreach (var otherOffer in otherActiveOffers)
        {
            otherOffer.Status =
                QuotationOfferStatus.Superseded;
        }

        offer.Status = QuotationOfferStatus.Accepted;
        offer.RespondedDate = DateTime.UtcNow;

        offer.Quotation.AcceptedOfferId =
            offer.QuotationOfferId;

        offer.Quotation.Status =
            QuotationStatus.Accepted;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            $"Revised offer {offer.OfferNumber} accepted successfully.";

        return RedirectToAction(
            nameof(QuotationDetails),
            new { id = offer.QuotationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOffer(
        int id,
        QuotationRejectionReason? rejectionReason,
        decimal? customerExpectedPrice,
        string? customerRejectionComment)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var offer = await _context.QuotationOffers
            .Include(o => o.Quotation)
            .FirstOrDefaultAsync(o =>
                o.QuotationOfferId == id &&
                o.Quotation.CustomerId == customer.CustomerId);

        if (offer is null)
            return NotFound();

        if (offer.Status != QuotationOfferStatus.Sent)
        {
            TempData["Error"] =
                "Only a sent revised offer can be rejected.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id = offer.QuotationId });
        }

        if (rejectionReason is null)
        {
            TempData["Error"] =
                "Please select a reason for rejecting the revised offer.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id = offer.QuotationId });
        }

        if (customerExpectedPrice is not null &&
            customerExpectedPrice < 0)
        {
            TempData["Error"] =
                "Expected price cannot be negative.";

            return RedirectToAction(
                nameof(QuotationDetails),
                new { id = offer.QuotationId });
        }

        offer.Status = QuotationOfferStatus.Rejected;
        offer.RejectionReason = rejectionReason;
        offer.CustomerExpectedPrice = customerExpectedPrice;
        offer.CustomerRejectionComment =
            string.IsNullOrWhiteSpace(customerRejectionComment)
                ? null
                : customerRejectionComment.Trim();
        offer.RejectedDate = DateTime.UtcNow;
        offer.RespondedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            $"Revised offer {offer.OfferNumber} rejected. Your feedback has been sent to the supplier.";

        return RedirectToAction(
            nameof(QuotationDetails),
            new { id = offer.QuotationId });
    }

    public async Task<IActionResult> QuotationDetails(int id)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        // Ownership check happens in the query itself, not after the fact —
        // a quotation belonging to another customer simply doesn't match
        // and comes back as 404, never as a 403 that confirms it exists.
        var quotation = await _context.Quotations
            .Include(q => q.Details)
                .ThenInclude(d => d.Product)
            .Include(q => q.Offers)
                .ThenInclude(o => o.Details)
                    .ThenInclude(d => d.Product)
            .Include(q => q.Orders)
            .FirstOrDefaultAsync(q =>
                q.QuotationId == id &&
                q.CustomerId == customer.CustomerId);

        if (quotation is null)
            return NotFound();

        var convertedOrder =
            quotation.Orders.FirstOrDefault();

        var vm = new MyQuotationDetailsViewModel
        {
            QuotationId = quotation.QuotationId,
            QuotationNumber = quotation.QuotationNumber,
            RequestDate = quotation.RequestDate,
            Status = quotation.Status,
            DeliveryLocation = quotation.DeliveryLocation,
            ValidUntil = quotation.ValidUntil,

            Lines = quotation.Details
                .Select(d => new MyQuotationLineViewModel
                {
                    ProductName = d.Product.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.TotalPrice
                })
                .ToList(),

            SubTotal =
                quotation.Details.Sum(d =>
                    d.TotalPrice ?? 0),

            DiscountAmount = quotation.DiscountAmount,
            TotalAmount = quotation.TotalAmount,
            RejectionReason = quotation.RejectionReason,
            CustomerExpectedPrice =
                quotation.CustomerExpectedPrice,
            CustomerRejectionComment =
                quotation.CustomerRejectionComment,
            RejectedDate = quotation.RejectedDate,
            OrderId = convertedOrder?.OrderId,
            OrderNumber = convertedOrder?.OrderNumber,

            Offers = quotation.Offers
                .Where(o =>
                    o.Status != QuotationOfferStatus.Draft)
                .OrderByDescending(o =>
                    o.RevisionNumber)
                .Select(o =>
                    new MyQuotationOfferViewModel
                    {
                        QuotationOfferId =
                            o.QuotationOfferId,
                        OfferNumber = o.OfferNumber,
                        RevisionNumber =
                            o.RevisionNumber,
                        Status = o.Status,
                        CreatedDate = o.CreatedDate,
                        SentDate = o.SentDate,
                        RespondedDate =
                            o.RespondedDate,
                        ValidUntil = o.ValidUntil,

                        Lines = o.Details
                            .Select(d =>
                                new MyQuotationOfferLineViewModel
                                {
                                    ProductName =
                                        d.Product.ProductName,
                                    Quantity = d.Quantity,
                                    UnitPrice =
                                        d.UnitPrice,
                                    LineTotal =
                                        d.TotalPrice
                                })
                            .ToList(),

                        SubTotal =
                            o.Details.Sum(d =>
                                d.TotalPrice ?? 0),

                        DiscountAmount =
                            o.DiscountAmount,

                        DeliveryCost =
                            o.DeliveryCost,

                        TotalAmount =
                            o.TotalAmount,

                        RejectionReason =
                            o.RejectionReason,

                        CustomerExpectedPrice =
                            o.CustomerExpectedPrice,

                        CustomerRejectionComment =
                            o.CustomerRejectionComment,

                        RejectedDate =
                            o.RejectedDate
                    })
                .ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Orders()
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

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

        if (customer is null)
            return RedirectToAction(nameof(Login));

        // Same pattern as QuotationDetails: ownership is part of the query,
        // not a check bolted on afterwards. order.Customer.UserId is never
        // trusted from a route value — only from the signed-in principal.
        var order = await _context.Orders
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o =>
                o.OrderId == id &&
                o.CustomerId == customer.CustomerId);

        if (order is null)
            return NotFound();

        var vm = new MyOrderDetailsViewModel
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            DeliveryLocation =
                order.DeliveryLocation,

            Lines = order.Details
                .Select(d => new MyOrderLineViewModel
                {
                    ProductName =
                        d.Product.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice
                })
                .ToList(),

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

    public async Task<IActionResult> Updates()
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var notifications =
            await _customerNotificationService.GetForCurrentUserAsync(User);

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenNotification(int id)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Forbid();

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.NotificationId == id &&
                n.UserId == userId);

        if (notification is null)
            return NotFound();

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(notification.Url) &&
            Url.IsLocalUrl(notification.Url))
        {
            return Redirect(notification.Url);
        }

        return RedirectToAction(nameof(Updates));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllNotificationsRead()
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        await _customerNotificationService.MarkAllAsReadAsync(User);

        return RedirectToAction(nameof(Updates));
    }

    public async Task<IActionResult> Profile()
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        var countryCode =
            _phoneNumberService.GetRegionCode(
                customer.Phone) ?? "UG";

        PopulateCountryOptions();

        return View(new ProfileViewModel
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            OrganizationName =
                customer.OrganizationName,
            Email = customer.Email ?? string.Empty,
            CountryCode = countryCode,
            Phone = GetNationalPhoneNumber(customer.Phone),
            Address = customer.Address,
            City = customer.City
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        ProfileViewModel vm)
    {
        var customer = await GetCurrentCustomerAsync();

        if (customer is null)
            return RedirectToAction(nameof(Login));

        vm.Email = customer.Email ?? string.Empty;

        if (ModelState.IsValid)
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
            }
            else
            {
                customer.FirstName = vm.FirstName;
                customer.LastName = vm.LastName;
                customer.OrganizationName =
                    vm.OrganizationName;
                customer.Phone = normalizedPhone;
                customer.Address = vm.Address;
                customer.City = vm.City;

                var user =
                    await _userManager.GetUserAsync(User);

                if (user is not null)
                {
                    user.PhoneNumber = normalizedPhone;

                    var updateResult =
                        await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                error.Description);
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Profile updated.";

                    return RedirectToAction(
                        nameof(Profile));
                }
            }
        }

        PopulateCountryOptions();

        return View(vm);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var user =
            await _userManager.GetUserAsync(User);

        if (user is null)
            return RedirectToAction(nameof(Login));

        var result =
            await _userManager.ChangePasswordAsync(
                user,
                vm.CurrentPassword,
                vm.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(vm);
        }

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(vm);
            }
        }

        TempData["Success"] =
            "Your password has been changed successfully.";

        return RedirectToAction(nameof(Profile));
    }

    private async Task<Customer?> GetCurrentCustomerAsync()
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return null;

        return await _context.Customers
            .FirstOrDefaultAsync(c =>
                c.UserId == userId);
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

    private IActionResult RedirectToLocalOrDashboard(
        string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
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

        return RedirectToAction(
            nameof(Dashboard));
    }
}
