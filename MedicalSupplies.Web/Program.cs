using MedicalSupplies.Infrastructure;
using MedicalSupplies.Web.Data;
using MedicalSupplies.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IQuotationCartService, QuotationCartService>();
builder.Services.AddScoped<IOrderFulfillmentService, OrderFulfillmentService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.IsEssential = true;
});

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
