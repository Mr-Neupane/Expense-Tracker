using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Controllers;

[AllowAnonymous]
public class MigrationController(
    ApplicationDbContext context,
    UserManager<AppUser> userManager,
    IToastNotification toastNotification)
    : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public async Task<RedirectResult> ApplyMigration()
    {
        try
        {
            var existingUser = await userManager.FindByIdAsync(UserConstants.AdminUser.ToString());
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    Id = UserConstants.AdminUser,
                    UserName = "Admin User",
                    DisplayName = "Admin User"
                };
                var createResult = await userManager.CreateAsync(user, "Admin@123");
                if (!createResult.Succeeded)
                {
                    toastNotification.AddErrorToastMessage(
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return Redirect("/Migration/Index");
                }
            }

            var existingCoaLedger =
                await context.CoaLedger.Select(x => x.RecStatus == Status.Active.ToInt()).CountAsync();
            if (existingCoaLedger == 0)
            {
                var initialCoa = new List<Coa>
                {
                    new()
                    {
                        Id = CoaConstants.Assets,
                        Name = "Assets"
                    },
                    new()
                    {
                        Id = CoaConstants.Liabilities,
                        Name = "Liabilities"
                    },
                    new()
                    {
                        Id = CoaConstants.Income,
                        Name = "Income"
                    },
                    new()
                    {
                        Id = CoaConstants.Expenses,
                        Name = "Expenses"
                    }
                };
                await context.CoaLedger.AddRangeAsync(initialCoa);
                await context.SaveChangesAsync();
            }

            var defaultParentLedger = await context.Ledgers.Where(x => x.Id == LedgerConstants.CashAccount).SingleOrDefaultAsync();
            if (defaultParentLedger == null)
            {
                var parentLedgers = new List<Ledger>
                {
                    new()
                    {
                        Id = LedgerConstants.CashAccount,
                        ParentId = CoaConstants.Assets,
                        LedgerName = "Cash Account",
                        Code = "80",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.BankAccount,
                        ParentId = CoaConstants.Assets,
                        LedgerName = "Bank Account",
                        Code = "90",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.CurrentLiabilities,
                        ParentId = CoaConstants.Liabilities,
                        LedgerName = "Current Liabilities",
                        Code = "60",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.OtherLiabilities,
                        ParentId = CoaConstants.Liabilities,
                        LedgerName = "Other Liabilities",
                        Code = "70",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.OtherIncome,
                        ParentId = CoaConstants.Income,
                        LedgerName = "Other Income",
                        Code = "160.1",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.InvestmentInterest,
                        ParentId = CoaConstants.Income,
                        LedgerName = "Investment Interest",
                        Code = "160.2",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.OtherExpenses,
                        ParentId = CoaConstants.Expenses,
                        LedgerName = "Other Expenses",
                        Code = "150.1",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.InterestExpenses,
                        ParentId = CoaConstants.Expenses,
                        LedgerName = "Interest Expenses",
                        Code = "150.2",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = LedgerConstants.Cash,
                        ParentId = null,
                        LedgerName = "Cash",
                        Code = "80.1",
                        SubParentId = LedgerConstants.CashAccount
                    }
                };
                await context.Ledgers.AddRangeAsync(parentLedgers);
                await context.SaveChangesAsync();
            }

            toastNotification.AddSuccessToastMessage("Migration applied successfully.");
            return Redirect("/");
        }
        catch (Exception e)
        {
            toastNotification.AddErrorToastMessage(e.Message);
            return Redirect("/Migration/Index");
        }
    }
}
