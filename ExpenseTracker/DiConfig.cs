using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Manager;
using ExpenseTracker.Manager.Interfaces;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.Providers.Interfaces;
using ExpenseTracker.Services;
using ExpenseTracker.UnitOfWork;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Middlewares;
using NToastNotify;
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

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(AppConstants.CookieExpireDays);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRazorPages();
        builder.UseServices();
        builder.UseRepo();
        builder.UseProviders();
        builder.UseNotificationServices();
    }

    private static void UseNotificationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()));
                options.Filters.Add<LoggingExceptionFilter>();
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
        builder.Services.AddScoped<IUow, Uow>();
        builder.Services.AddScoped<IVoucherService, VoucherService>();
        builder.Services.AddScoped<IBankService, BankService>();
        builder.Services.AddScoped<IIncomeService, IncomeService>();
        builder.Services.AddScoped<IExpenseService, ExpenseService>();
        builder.Services.AddScoped<ILiabilityService, LiabilityService>();
        builder.Services.AddScoped<ILedgerService, LedgerService>();
        builder.Services.AddScoped<IAccTransactionManager,AccTransactionManager>();
        builder.Services.AddScoped<ReverseTransactionManager>();
    }

    private static void UseRepo(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBankRepo, BankRepo>();
        builder.Services.AddScoped<IBankTransactionRepo, BankTransactionRepo>();
        builder.Services.AddScoped<ICoaLedgerRepo, CoaLedgerRepo>();
        builder.Services.AddScoped<IExpenseRepo, ExpenseRepo>();
        builder.Services.AddScoped<IIncomeRepo, IncomeRepo>();
        builder.Services.AddScoped<ILedgerRepo, LedgerRepo>();
        builder.Services.AddScoped<ILiabilityRepo, LiabilityRepo>();
        builder.Services.AddScoped<IAccountingTransactionRepo, AccountingTransactionRepo>();
        builder.Services.AddScoped<IAccTxnDetailRepo, AccTxnDetailRepo>();
        builder.Services.AddScoped<IUserRepo, UserRepo>();
        builder.Services.AddScoped<IRoleRepo, RoleRepo>();
        builder.Services.AddScoped<IUserRoleRepo, UserRoleRepo>();
    }

    private static void UseProviders(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        builder.Services.AddScoped<IProvider>();
        builder.Services.AddScoped<IBalanceProvider>();
        builder.Services.AddScoped<DropdownProvider>();
        builder.Services.AddScoped<IParentLedgerProvider,ParentLedgerProvider>();
    }
}
