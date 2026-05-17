using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TestApplication.Enums;

namespace ExpenseTracker.Controllers;

public class MigrationController : Controller
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
            var existingUser = await _userGenericRepo.SingleOrDefaultAsync(x => x.Id == -1);
            if (existingUser == null)
            {
                var user = new User
                {
                    Id = -1,
                    Username = "Admin User",
                    Password = "Admin@123"
                };
                await _uow.AddAsync(user);
                await _uow.SaveChangesAsync();
            }

            var existingCoaLedger = await _coaGenericRepo.GetBaseQueryable()
                .CountAsync(x => x.RecStatus == Status.Active.ToInt());
            if (existingCoaLedger == 0)
            {
                var initialCoa = new List<Coa>
                {
                    new() { Id = 1, Name = "Assets" },
                    new() { Id = 2, Name = "Liabilities" },
                    new() { Id = 3, Name = "Income" },
                    new() { Id = 4, Name = "Expenses" }
                };
                foreach (var coa in initialCoa)
                    await _uow.AddAsync(coa);
                await _uow.SaveChangesAsync();
            }

            var defaultParentLedger = await _ledgerGenericRepo.SingleOrDefaultAsync(x => x.Id == -1);
            if (defaultParentLedger == null)
            {
                var parentLedgers = new List<Ledger>
                {
                    new() { Id = -1, ParentId = 1, LedgerName = "Cash Account", Code = "80", SubParentId = null },
                    new() { Id = -2, ParentId = 1, LedgerName = "Bank Account", Code = "90", SubParentId = null },
                    new() { Id = -4, ParentId = 2, LedgerName = "Current Liabilities", Code = "60", SubParentId = null },
                    new() { Id = -5, ParentId = 2, LedgerName = "Other Liabilities", Code = "70", SubParentId = null },
                    new() { Id = -6, ParentId = 3, LedgerName = "Other Income", Code = "160.1", SubParentId = null },
                    new() { Id = -7, ParentId = 3, LedgerName = "Investment Interest", Code = "160.2", SubParentId = null },
                    new() { Id = -8, ParentId = 4, LedgerName = "Other Expenses", Code = "150.1", SubParentId = null },
                    new() { Id = -9, ParentId = 4, LedgerName = "Interest Expenses", Code = "150.2", SubParentId = null },
                    new() { Id = -3, ParentId = null, LedgerName = "Cash", Code = "80.1", SubParentId = -1 }
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
