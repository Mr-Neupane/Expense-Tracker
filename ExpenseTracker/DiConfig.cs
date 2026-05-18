using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Interface;
using ExpenseTracker.Manager;
using ExpenseTracker.Manager.Interfaces;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.Providers.Interfaces;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using ExpenseTracker.Interface;
using ExpenseTracker.ViewModels.Interface;

namespace ExpenseTracker;

public static class DiConfig
{
    public static void UseApp(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = AppConstants.PasswordMinLength;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromDays(AppConstants.CookieExpireDays);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRazorPages();
        builder.UseServices();
        builder.UseNotificationServices();
    }

    private static void UseNotificationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()));
            })
            .AddNToastNotifyToastr(new ToastrOptions
            {
                PositionClass = ToastPositions.BottomRight,
                CloseButton = true,
                TimeOut = AppConstants.ToastTimeoutMs
            });
    }

    private static void UseServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddScoped<IAuthManager, AuthManager>();
        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
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
