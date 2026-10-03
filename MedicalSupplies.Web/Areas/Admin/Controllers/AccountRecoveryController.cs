using System.Security.Cryptography;
using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Core.Security;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.Services;
using MedicalSupplies.Web.ViewModels.Admin.AccountRecovery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class AccountRecoveryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPhoneNumberService _phoneNumberService;
    private readonly ICustomerNotificationService _customerNotificationService;

    public AccountRecoveryController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IPhoneNumberService phoneNumberService,
        ICustomerNotificationService customerNotificationService)
    {
        _context = context;
        _userManager = userManager;
        _phoneNumberService = phoneNumberService;
        _customerNotificationService = customerNotificationService;
    }

    [Authorize(Policy = Permissions.AccountRecovery.View)]
    public async Task<IActionResult> Index(string? status)
    {
        var requests = _context.AccountRecoveryRequests
            .AsNoTracking()
            .Include(r => r.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.Trim();
            requests = requests.Where(r => r.Status == status);
        }

        var models = await requests
            .OrderByDescending(r => r.CreatedDate)
            .Select(r => new AccountRecoveryListItemViewModel
            {
                AccountRecoveryRequestId = r.AccountRecoveryRequestId,
                CustomerId = r.CustomerId,
                CustomerName =
                    ((r.Customer.FirstName ?? "") + " " +
                     (r.Customer.LastName ?? "")).Trim(),
                OrganizationName = r.Customer.OrganizationName,
                SubmittedEmail = r.SubmittedEmail,
                SubmittedPhone = r.SubmittedPhone,
                RecoveryType = r.RecoveryType,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                ReviewedBy = r.ReviewedBy,
                ReviewedDate = r.ReviewedDate
            })
            .ToListAsync();

        ViewBag.Status = status;

        return View(models);
    }

    [Authorize(Policy = Permissions.AccountRecovery.View)]
    public async Task<IActionResult> Details(int id)
    {
        var model = await BuildDetailsModelAsync(id);

        if (model is null)
            return NotFound();

        PopulateCountryOptions();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.AccountRecovery.Review)]
    public async Task<IActionResult> Approve(
        AccountRecoveryReviewViewModel vm)
    {
        var reviewerId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(reviewerId))
            return Forbid();

        var reviewer = await _userManager.FindByIdAsync(reviewerId);

        if (reviewer is null || !reviewer.IsActive)
            return Forbid();

        var request = await _context.AccountRecoveryRequests
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(
                r => r.AccountRecoveryRequestId ==
                     vm.AccountRecoveryRequestId);

        if (request is null)
            return NotFound();

        if (request.Customer is null)
        {
            TempData["Error"] =
                "The customer linked to this recovery request could not be found.";

            return RedirectToAction(
                nameof(Details),
                new { id = request.AccountRecoveryRequestId });
        }

        if (IsTerminalStatus(request.Status))
        {
            TempData["Error"] =
                "This recovery request has already been completed or rejected.";

            return RedirectToAction(
                nameof(Details),
                new { id = request.AccountRecoveryRequestId });
        }

        if (request.Customer.UserId is null)
        {
            TempData["Error"] =
                "This customer does not have a registered login account.";

            return RedirectToAction(
                nameof(Details),
                new { id = request.AccountRecoveryRequestId });
        }

        var user = await _userManager.FindByIdAsync(
            request.Customer.UserId);

        if (user is null)
        {
            TempData["Error"] =
                "The login account linked to this customer could not be found.";

            return RedirectToAction(
                nameof(Details),
                new { id = request.AccountRecoveryRequestId });
        }

        vm.Status = "Approved";

        if (string.IsNullOrWhiteSpace(vm.VerificationMethod))
        {
            ModelState.AddModelError(
                nameof(vm.VerificationMethod),
                "Verification method is required.");
        }

        if (string.IsNullOrWhiteSpace(vm.VerificationNotes))
        {
            ModelState.AddModelError(
                nameof(vm.VerificationNotes),
                "Verification notes are required.");
        }

        if (!string.IsNullOrWhiteSpace(vm.NewContactPhone))
        {
            if (string.IsNullOrWhiteSpace(vm.NewContactCountryCode))
            {
                ModelState.AddModelError(
                    nameof(vm.NewContactCountryCode),
                    "Please select the country for the new contact number.");
            }
            else if (!_phoneNumberService.TryNormalize(
                         vm.NewContactPhone,
                         vm.NewContactCountryCode,
                         out _,
                         out var phoneError))
            {
                ModelState.AddModelError(
                    nameof(vm.NewContactPhone),
                    phoneError);
            }
        }

        if (!ModelState.IsValid)
        {
            var details = await BuildDetailsModelAsync(
                request.AccountRecoveryRequestId);

            if (details is null)
                return NotFound();

            vm.CustomerName = details.Customer.CustomerName;
            vm.OrganizationName = details.Customer.OrganizationName;
            vm.SubmittedEmail = details.SubmittedEmail;
            vm.SubmittedPhone = details.SubmittedPhone;
            vm.Reason = details.Reason;

            ViewBag.ReviewModel = vm;
            PopulateCountryOptions();

            return View("Details", details);
        }

        string? normalizedNewPhone = null;

        if (!string.IsNullOrWhiteSpace(vm.NewContactPhone))
        {
            if (!_phoneNumberService.TryNormalize(
                    vm.NewContactPhone,
                    vm.NewContactCountryCode,
                    out normalizedNewPhone,
                    out var phoneError))
            {
                ModelState.AddModelError(
                    nameof(vm.NewContactPhone),
                    phoneError);

                var details = await BuildDetailsModelAsync(
                    request.AccountRecoveryRequestId);

                if (details is null)
                    return NotFound();

                vm.CustomerName = details.Customer.CustomerName;
                vm.OrganizationName = details.Customer.OrganizationName;
                vm.SubmittedEmail = details.SubmittedEmail;
                vm.SubmittedPhone = details.SubmittedPhone;
                vm.Reason = details.Reason;

                ViewBag.ReviewModel = vm;
                PopulateCountryOptions();

                return View("Details", details);
            }
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var token =
            await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetResult =
            await _userManager.ResetPasswordAsync(
                user,
                token,
                temporaryPassword);

        if (!resetResult.Succeeded)
        {
            foreach (var error in resetResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            var details = await BuildDetailsModelAsync(
                request.AccountRecoveryRequestId);

            if (details is null)
                return NotFound();

            ViewBag.ReviewModel = vm;
            PopulateCountryOptions();

            return View("Details", details);
        }

        user.MustChangePassword = true;

        if (!string.IsNullOrWhiteSpace(normalizedNewPhone))
        {
            request.NewContactPhone = normalizedNewPhone;
            request.Customer.Phone = normalizedNewPhone;
            user.PhoneNumber = normalizedNewPhone;
        }

        request.Status = "Approved";

        request.VerificationMethod =
            vm.VerificationMethod!.Trim();

        request.VerificationNotes =
            BuildVerificationNotes(
                vm.VerificationNotes,
                vm.ReviewerNotes);

        request.ReviewedBy =
            reviewer.FullName
            ?? reviewer.Email
            ?? reviewer.UserName
            ?? reviewer.Id;

        request.ReviewedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _customerNotificationService.CreateAsync(
            request.Customer.UserId!,
            "AccountRecoveryApproved",
            "Account recovery approved",
            "Your account recovery request has been approved. Please sign in and complete your password change.",
            "/Account/Login");

        var approvedDetails = await BuildDetailsModelAsync(
            request.AccountRecoveryRequestId);

        if (approvedDetails is null)
            return NotFound();

        ViewBag.TemporaryPassword = temporaryPassword;

        return View("Details", approvedDetails);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.AccountRecovery.Review)]
    public async Task<IActionResult> Reject(
        AccountRecoveryReviewViewModel vm)
    {
        var reviewerId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(reviewerId))
            return Forbid();

        var reviewer = await _userManager.FindByIdAsync(reviewerId);

        if (reviewer is null || !reviewer.IsActive)
            return Forbid();

        var request = await _context.AccountRecoveryRequests
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(
                r => r.AccountRecoveryRequestId ==
                     vm.AccountRecoveryRequestId);

        if (request is null)
            return NotFound();

        if (request.Customer is null)
        {
            TempData["Error"] =
                "The customer linked to this recovery request could not be found.";

            return RedirectToAction(
                nameof(Details),
                new { id = request.AccountRecoveryRequestId });
        }

        if (IsTerminalStatus(request.Status))
        {
            TempData["Error"] =
                "This recovery request has already been completed or rejected.";

            return RedirectToAction(
                nameof(Details),
                new { id = request.AccountRecoveryRequestId });
        }

        if (string.IsNullOrWhiteSpace(vm.VerificationNotes) &&
            string.IsNullOrWhiteSpace(vm.ReviewerNotes))
        {
            ModelState.AddModelError(
                nameof(vm.ReviewerNotes),
                "Please provide a reason or verification note for rejecting this request.");
        }

        if (!ModelState.IsValid)
        {
            var details = await BuildDetailsModelAsync(
                request.AccountRecoveryRequestId);

            if (details is null)
                return NotFound();

            ViewBag.ReviewModel = vm;
            PopulateCountryOptions();

            return View("Details", details);
        }

        request.Status = "Rejected";

        request.VerificationMethod =
            string.IsNullOrWhiteSpace(vm.VerificationMethod)
                ? null
                : vm.VerificationMethod!.Trim();

        request.VerificationNotes =
            BuildVerificationNotes(
                vm.VerificationNotes,
                vm.ReviewerNotes);

        request.ReviewedBy =
            reviewer.FullName
            ?? reviewer.Email
            ?? reviewer.UserName
            ?? reviewer.Id;

        request.ReviewedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.Customer.UserId))
        {
            await _customerNotificationService.CreateAsync(
                request.Customer.UserId,
                "AccountRecoveryRejected",
                "Account recovery update",
                "Your account recovery request has been rejected. Please review the update in your Business Account.",
                "/Account/Updates");
        }

        TempData["Success"] =
            "The account recovery request has been rejected.";

        return RedirectToAction(
            nameof(Details),
            new { id = request.AccountRecoveryRequestId });
    }

    private async Task<AccountRecoveryDetailsViewModel?> BuildDetailsModelAsync(
        int id)
    {
        var request = await _context.AccountRecoveryRequests
            .AsNoTracking()
            .Include(r => r.Customer)
                .ThenInclude(c => c.Quotations)
            .Include(r => r.Customer)
                .ThenInclude(c => c.Orders)
            .FirstOrDefaultAsync(
                r => r.AccountRecoveryRequestId == id);

        if (request is null || request.Customer is null)
            return null;

        var customer = request.Customer;

        var customerName =
            ((customer.FirstName ?? "") + " " +
             (customer.LastName ?? "")).Trim();

        if (string.IsNullOrWhiteSpace(customerName))
        {
            customerName =
                customer.OrganizationName
                ?? "Unnamed customer";
        }

        var hasLogin =
            !string.IsNullOrWhiteSpace(customer.UserId);

        return new AccountRecoveryDetailsViewModel
        {
            AccountRecoveryRequestId =
                request.AccountRecoveryRequestId,
            RecoveryType = request.RecoveryType,
            Status = request.Status,
            SubmittedEmail = request.SubmittedEmail,
            SubmittedPhone = request.SubmittedPhone,
            NewContactCountryCode =
                _phoneNumberService.GetRegionCode(
                    request.NewContactPhone),
            NewContactPhone = GetNationalPhoneNumber(
                request.NewContactPhone),
            Reason = request.Reason,
            CreatedDate = request.CreatedDate,
            ReviewedBy = request.ReviewedBy,
            ReviewedDate = request.ReviewedDate,
            CompletedDate = request.CompletedDate,
            VerificationMethod = request.VerificationMethod,
            VerificationNotes = request.VerificationNotes,
            Customer = new CustomerRecoveryDetailsViewModel
            {
                CustomerId = customer.CustomerId,
                OrganizationName = customer.OrganizationName,
                CustomerName = customerName,
                Email = customer.Email,
                Phone = customer.Phone,
                CustomerType = customer.CustomerType.ToString(),
                Address = customer.Address,
                City = customer.City,
                IsActive = customer.IsActive,
                HasLogin = hasLogin
            },
            Quotations = customer.Quotations
                .OrderByDescending(q => q.RequestDate)
                .Select(q => new CustomerRecoveryQuotationViewModel
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
                .Select(o => new CustomerRecoveryOrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.OrderStatus.ToString(),
                    TotalAmount = o.TotalAmount
                })
                .ToList()
        };
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

    private static bool IsTerminalStatus(string? status)
    {
        return string.Equals(
                   status,
                   "Rejected",
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   status,
                   "Completed",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string? BuildVerificationNotes(
        string? verificationNotes,
        string? reviewerNotes)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(verificationNotes))
        {
            parts.Add(
                "Verification: " +
                verificationNotes.Trim());
        }

        if (!string.IsNullOrWhiteSpace(reviewerNotes))
        {
            parts.Add(
                "Reviewer: " +
                reviewerNotes.Trim());
        }

        return parts.Count == 0
            ? null
            : string.Join(
                Environment.NewLine + Environment.NewLine,
                parts);
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper =
            "ABCDEFGHJKLMNPQRSTUVWXYZ";

        const string lower =
            "abcdefghijkmnopqrstuvwxyz";

        const string numbers =
            "23456789";

        const string symbols =
            "!@#$%&*";

        var passwordChars = new List<char>
        {
            GetRandomCharacter(upper),
            GetRandomCharacter(lower),
            GetRandomCharacter(numbers),
            GetRandomCharacter(symbols)
        };

        const string all =
            upper + lower + numbers + symbols;

        while (passwordChars.Count < 12)
        {
            passwordChars.Add(
                GetRandomCharacter(all));
        }

        for (var i = passwordChars.Count - 1; i > 0; i--)
        {
            var j =
                RandomNumberGenerator.GetInt32(i + 1);

            (passwordChars[i], passwordChars[j]) =
                (passwordChars[j], passwordChars[i]);
        }

        return new string(passwordChars.ToArray());
    }

    private static char GetRandomCharacter(
        string characters)
    {
        var index =
            RandomNumberGenerator.GetInt32(
                characters.Length);

        return characters[index];
    }
}
