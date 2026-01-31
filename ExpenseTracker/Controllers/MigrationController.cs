using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TestApplication.Enums;

namespace ExpenseTracker.Controllers;

public class MigrationController(ApplicationDbContext context, IToastNotification toastNotification)
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
            var existingUser = await context.Users.Where(x => x.Id == -1).SingleOrDefaultAsync();
            if (existingUser == null)
            {
                var user = new User
                {
                    Id = -1,
                    Username = "Admin User",
                    Password = "Admin@123"
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            var existingCoaLedger =
                await context.CoaLedger.Select(x => x.RecStatus == Status.Active.ToInt()).CountAsync();
            if (existingCoaLedger == 0)
            {
                var initialCoa = new List<Coa>
                {
                    new()
                    {
                        Id = 1,
                        Name = "Assets"
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Liabilities"
                    },
                    new()
                    {
                        Id = 3,
                        Name = "Income"
                    },
                    new()
                    {
                        Id = 4,
                        Name = "Expenses"
                    }
                };
                await context.CoaLedger.AddRangeAsync(initialCoa);
                await context.SaveChangesAsync();
            }


            var defaultParentLedger = await context.Ledgers.Where(x => x.Id == -1).SingleOrDefaultAsync();
            if (defaultParentLedger == null)
            {
                var parentLedgers = new List<Ledger>
                {
                    new()
                    {
                        Id = -1,
                        ParentId = 1,
                        LedgerName = "Cash Account",
                        Code = "80",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -2,
                        ParentId = 1,
                        LedgerName = "Bank Account",
                        Code = "90",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -4,
                        ParentId = 2,
                        LedgerName = "Current Liabilities",
                        Code = "60",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -5,
                        ParentId = 2,
                        LedgerName = "Other Liabilities",
                        Code = "70",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -6,
                        ParentId = 3,
                        LedgerName = "Other Income",
                        Code = "160.1",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -7,
                        ParentId = 3,
                        LedgerName = "Investment Interest",
                        Code = "160.2",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -8,
                        ParentId = 4,
                        LedgerName = "Other Expenses",
                        Code = "150.1",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -9,
                        ParentId = 4,
                        LedgerName = "Interest Expenses",
                        Code = "150.2",
                        SubParentId = null
                    },
                    new()
                    {
                        Id = -3,
                        ParentId = null,
                        LedgerName = "Cash",
                        Code = "80.1",
                        SubParentId = -1
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
            return Redirect($"Home/Migration/");
        }
    }
}