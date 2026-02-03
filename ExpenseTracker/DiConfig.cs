using ExpenseTracker.Data;
using ExpenseTracker.Manager;
using ExpenseTracker.Providers;
using ExpenseTracker.Services;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker;

public static class DiConfig
{
    public static void UseApp(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        builder.Services.AddRazorPages();

        builder.UseServices();

        builder.UseNotificationServices();
    }

    private static void UseNotificationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews()
            .AddNToastNotifyToastr(new ToastrOptions
            {
                PositionClass = ToastPositions.BottomRight,
                CloseButton = true,
                TimeOut = 5000
            });
    }

    private static void UseServices(this WebApplicationBuilder builder)
    {
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
    }
}