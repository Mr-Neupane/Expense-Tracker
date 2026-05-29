using ExpenseTracker.Constants;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTracker.Controllers;

public class MigrationController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICoaLedgerRepo _coaGenericRepo;
    private readonly ILedgerRepo _ledgerGenericRepo;
    private readonly IUow _uow;
    private readonly IToastNotification _toastNotification;

    public MigrationController(UserManager<AppUser> userManager, ICoaLedgerRepo coaGenericRepo,
        ILedgerRepo ledgerGenericRepo, IUow uow, IToastNotification toastNotification)
    {
        _userManager = userManager;
        _coaGenericRepo = coaGenericRepo;
        _ledgerGenericRepo = ledgerGenericRepo;
        _uow = uow;
        _toastNotification = toastNotification;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }
    [AllowAnonymous]
    public async Task<IActionResult> ApplyMigration()
    {
        try
        {
            var existingUser = await _userManager.FindByIdAsync(UserConstants.AdminUser.ToString());
            if (existingUser == null)
            {
                var adminUser = new AppUser
                {
                    Id = UserConstants.AdminUser,
                    UserName = "AdminUser",
                    DisplayName = "Admin User"
                };
                var result = await _userManager.CreateAsync(adminUser, "Admin@123");
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var existingCoaLedger = await _coaGenericRepo.GetBaseQueryable()
                .CountAsync(x => x.RecStatus == RecordStatusConstants.Active);
            if (existingCoaLedger == 0)
            {
                var initialCoa = new List<Coa>
                {
                    new() { Id = CoaConstants.Assets, Name = "Assets" },
                    new() { Id = CoaConstants.Liabilities, Name = "Liabilities" },
                    new() { Id = CoaConstants.Income, Name = "Income" },
                    new() { Id = CoaConstants.Expenses, Name = "Expenses" }
                };
                foreach (var coa in initialCoa)
                    await _uow.AddAsync(coa);
                await _uow.SaveChangesAsync();
            }

            var defaultParentLedger = await _ledgerGenericRepo.SingleOrDefaultAsync(x => x.Id == LedgerConstants.CashAccount);
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
                foreach (var ledger in parentLedgers)
                    await _uow.AddAsync(ledger);
                await _uow.SaveChangesAsync();
            }

            _toastNotification.AddSuccessToastMessage("Migration applied successfully.");
            return Redirect("/");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            return RedirectToAction("Index");
        }
    }
}
