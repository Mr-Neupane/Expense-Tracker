<<<<<<< HEAD
using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
=======
﻿using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
>>>>>>> main
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Controllers;

<<<<<<< HEAD
[AllowAnonymous]
public class MigrationController(
    ApplicationDbContext context,
    UserManager<AppUser> userManager,
    IToastNotification toastNotification)
    : Controller
=======
public class MigrationController : Controller
>>>>>>> main
{
    private readonly IUserGenericRepository _userGenericRepo;
    private readonly ICoaGenericRepository _coaGenericRepo;
    private readonly ILedgerGenericRepository _ledgerGenericRepo;
    private readonly IUow _uow;
    private readonly IToastNotification _toastNotification;

    public MigrationController(IUserGenericRepository userGenericRepo, ICoaGenericRepository coaGenericRepo,
        ILedgerGenericRepository ledgerGenericRepo, IUow uow, IToastNotification toastNotification)
    {
        _userGenericRepo = userGenericRepo;
        _coaGenericRepo = coaGenericRepo;
        _ledgerGenericRepo = ledgerGenericRepo;
        _uow = uow;
        _toastNotification = toastNotification;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> ApplyMigration()
    {
        try
        {
<<<<<<< HEAD
            var existingUser = await userManager.FindByIdAsync(UserConstants.AdminUser.ToString());
=======
            var existingUser = await _userGenericRepo.SingleOrDefaultAsync(x => x.Id == -1);
>>>>>>> main
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    Id = UserConstants.AdminUser,
                    UserName = "Admin User",
                    DisplayName = "Admin User"
                };
<<<<<<< HEAD
                var createResult = await userManager.CreateAsync(user, "Admin@123");
                if (!createResult.Succeeded)
                {
                    toastNotification.AddErrorToastMessage(
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return Redirect("/Migration/Index");
                }
=======
                await _uow.AddAsync(user);
                await _uow.SaveChangesAsync();
>>>>>>> main
            }

            var existingCoaLedger = await _coaGenericRepo.GetBaseQueryable()
                .CountAsync(x => x.RecStatus == Status.Active.ToInt());
            if (existingCoaLedger == 0)
            {
                var initialCoa = new List<Coa>
                {
<<<<<<< HEAD
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
=======
                    new() { Id = 1, Name = "Assets" },
                    new() { Id = 2, Name = "Liabilities" },
                    new() { Id = 3, Name = "Income" },
                    new() { Id = 4, Name = "Expenses" }
>>>>>>> main
                };
                foreach (var coa in initialCoa)
                    await _uow.AddAsync(coa);
                await _uow.SaveChangesAsync();
            }

<<<<<<< HEAD
            var defaultParentLedger = await context.Ledgers.Where(x => x.Id == LedgerConstants.CashAccount).SingleOrDefaultAsync();
=======
            var defaultParentLedger = await _ledgerGenericRepo.SingleOrDefaultAsync(x => x.Id == -1);
>>>>>>> main
            if (defaultParentLedger == null)
            {
                var parentLedgers = new List<Ledger>
                {
<<<<<<< HEAD
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
=======
                    new() { Id = -1, ParentId = 1, LedgerName = "Cash Account", Code = "80", SubParentId = null },
                    new() { Id = -2, ParentId = 1, LedgerName = "Bank Account", Code = "90", SubParentId = null },
                    new() { Id = -4, ParentId = 2, LedgerName = "Current Liabilities", Code = "60", SubParentId = null },
                    new() { Id = -5, ParentId = 2, LedgerName = "Other Liabilities", Code = "70", SubParentId = null },
                    new() { Id = -6, ParentId = 3, LedgerName = "Other Income", Code = "160.1", SubParentId = null },
                    new() { Id = -7, ParentId = 3, LedgerName = "Investment Interest", Code = "160.2", SubParentId = null },
                    new() { Id = -8, ParentId = 4, LedgerName = "Other Expenses", Code = "150.1", SubParentId = null },
                    new() { Id = -9, ParentId = 4, LedgerName = "Interest Expenses", Code = "150.2", SubParentId = null },
                    new() { Id = -3, ParentId = null, LedgerName = "Cash", Code = "80.1", SubParentId = -1 }
>>>>>>> main
                };
                foreach (var ledger in parentLedgers)
                    await _uow.AddAsync(ledger);
                await _uow.SaveChangesAsync();
            }

            _toastNotification.AddSuccessToastMessage("Migration applied successfully.");
            return Redirect("/");
        }
        catch (Exception e)
        {
<<<<<<< HEAD
            toastNotification.AddErrorToastMessage(e.Message);
            return Redirect("/Migration/Index");
=======
            _toastNotification.AddErrorToastMessage(e.Message);
            return RedirectToAction("Index");
>>>>>>> main
        }
    }
}
