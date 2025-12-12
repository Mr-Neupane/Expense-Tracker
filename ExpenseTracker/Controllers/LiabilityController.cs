using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class LiabilityController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public LiabilityController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET
    public IActionResult AddLiability()
    {
        return View();
    }

    [HttpPost]
    public async Task<RedirectToActionResult> AddLiability(LiabilityVm vm)
    {
        var engdate = DateTime.SpecifyKind(await DateHelper.GetEnglishDate(vm.TxnDate), DateTimeKind.Utc);
        var liab = new Liability
        {
            LedgerId = vm.LiabilityLedger,
            DrAmount = 0,
            CrAmount = vm.Amount,
            TxnDate = engdate,
            RecDate = DateTime.UtcNow,
            RecStatus = vm.RecStatus,
            Status = vm.Status,
            RecById = vm.RecById,
        };
        await _dbContext.Liabilities.AddAsync(liab);
        await _dbContext.SaveChangesAsync();

        await VoucherController.GetInsertedAccountingId(new AccountingTxn
        {
            TxnDate = engdate,
            DrAmount = 0,
            CrAmount = vm.Amount,
            Type = "Liability",
            TypeID = liab.Id,
            FromLedgerID = vm.LiabilityLedger,
            ToLedgerID = vm.LiabilityFromLedger,
            Remarks = vm.Remarks,
        });
        return RedirectToAction("AccountingTransaction", "Voucher");
    }
}