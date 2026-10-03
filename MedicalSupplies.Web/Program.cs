using MedicalSupplies.Infrastructure;
using MedicalSupplies.Web.Data;
using MedicalSupplies.Web.Services;
using MedicalSupplies.Web.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<
    Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

builder.Services.AddScoped<
    Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    PermissionAuthorizationHandler>();

builder.Services.AddScoped<PermissionService>();

builder.Services.AddScoped<IPhoneNumberService, PhoneNumberService>();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IQuotationCartService, QuotationCartService>();
builder.Services.AddScoped<IOrderFulfillmentService, OrderFulfillmentService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<ICustomerNotificationService, CustomerNotificationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var path = context.Request.Path;

        var isChangePassword =
            path.StartsWithSegments("/Account/ChangePassword");

        var isLogout =
            path.StartsWithSegments("/Account/Logout");

        if (!isChangePassword && !isLogout)
        {
            var userManager =
                context.RequestServices.GetRequiredService<
                    Microsoft.AspNetCore.Identity.UserManager<
                        MedicalSupplies.Infrastructure.Identity.ApplicationUser>>();

            var user = await userManager.GetUserAsync(context.User);

            if (user?.MustChangePassword == true)
            {
                context.Response.Redirect("/Account/ChangePassword");
                return;
            }
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create the application roles on startup.
await SeedData.SeedRolesAsync(app.Services);

// One-time CLI setup for the first SuperAdmin account.
if (args.Contains("--create-superadmin", StringComparer.OrdinalIgnoreCase))
{
    await AdminBootstrap.CreateAsync(app.Services);
    return;
}

app.Run();
