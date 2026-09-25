using MedicalSupplies.Infrastructure;
using MedicalSupplies.Web.Data;
using MedicalSupplies.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Data layer, Identity and repositories (see MedicalSupplies.Infrastructure/DependencyInjection.cs)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllersWithViews();

// Product image uploads (admin) and the "request a quote" session cart (public)
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

// Lets the admin dashboard live under /Admin/... via an ASP.NET Core Area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Creates the SuperAdmin/Admin/Sales/InventoryManager/Customer roles on startup
await SeedData.SeedRolesAsync(app.Services);

app.Run();
