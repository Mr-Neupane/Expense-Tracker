using Dapper;
using ExpenseTracker;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.Services;
using NToastNotify;
using TestApplication.ViewModels.Interface;

var builder = WebApplication.CreateBuilder(args);

// builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IVoucherService, VoucherService>();


builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        PositionClass = ToastPositions.BottomRight,
        CloseButton = true,
        TimeOut = 5000
    });


DapperConnectionProvider.Initialize(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.UseNToastNotify();

// Run Seeded Query During Build
await SeededData.SeededQuery();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();