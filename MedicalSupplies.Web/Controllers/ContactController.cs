using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Infrastructure.Identity;
using MedicalSupplies.Web.ViewModels.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ContactController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var vm = new ContactFormViewModel();

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not null)
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (customer is not null)
                {
                    vm.Name =
                        $"{customer.FirstName} {customer.LastName}".Trim();

                    vm.Email = customer.Email;
                    vm.Phone = customer.Phone;
                }
                else
                {
                    vm.Name = user.FullName?.Trim() ?? string.Empty;
                    vm.Email = user.Email;
                    vm.Phone = user.PhoneNumber;
                }
            }
        }

        return View(vm);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        Customer? customer = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is not null)
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
            }
        }

        var enquiry = new Enquiry
        {
            CustomerId = customer?.CustomerId,
            Name = vm.Name.Trim(),
            Email = string.IsNullOrWhiteSpace(vm.Email)
                ? null
                : vm.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(vm.Phone)
                ? null
                : vm.Phone.Trim(),
            Subject = string.IsNullOrWhiteSpace(vm.Subject)
                ? null
                : vm.Subject.Trim(),
            Message = vm.Message.Trim(),
            Status = EnquiryStatus.New,
            CreatedDate = DateTime.UtcNow
        };

        _context.Enquiries.Add(enquiry);
        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Thank you. Your enquiry has been received and our team will get back to you.";

        return RedirectToAction(nameof(Index));
    }
}
