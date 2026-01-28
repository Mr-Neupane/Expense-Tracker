using ExpenseTracker;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.Manager;
using ExpenseTracker.Providers;
using ExpenseTracker.Services;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.Manager;
using TestApplication.ViewModels.Interface;

var builder = WebApplication.CreateBuilder(args);

// builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddRazorPages();


builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ILiabilityService, LiabilityService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<AccTransactionManager>();
builder.Services.AddScoped<DropdownProvider>();
builder.Services.AddScoped<IProvider>();
builder.Services.AddScoped<IBalanceProvider>();
builder.Services.AddScoped<ReverseTransactionManager>();

builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        PositionClass = ToastPositions.BottomRight,
        CloseButton = true,
        TimeOut = 5000
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

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
// await SeededData.SeededQuery();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();