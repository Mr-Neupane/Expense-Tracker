using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
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

        var txnid = await VoucherController.GetInsertedAccountingId(new AccountingTxn
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

        var bankid = await BankService.GetBankIdByLedgerId(vm.LiabilityFromLedger);

        if (bankid != 0)
        {
            int banktransaction = await BankService.RecordBankTransaction(new BankTransactionVm
            {
                RecStatus = vm.RecStatus,
                Status = vm.Status,
                RecById = vm.RecById,
                BankId = bankid,
                TxnDate = vm.TxnDate,
                Amount = vm.Amount,
                Remarks = vm.Remarks,
                Type = "Deposit"
            });
            await BankService.UpdateTransactionDuringBankTransaction(banktransaction, txnid);
        }

        return RedirectToAction("AccountingTransaction", "Voucher");
    }

    [HttpGet]
    public async Task<IActionResult> LiabilityReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select e.*, voucher_no, username,t.id as transactionid
from accounting.liability e
         join accounting.transactions t on t.type_id = e.id
         join users u on e.rec_by_id = u.id
where t.type = 'Liability'
  and e.status = 1
  and t.status = 1";
        var report = await conn.QueryAsync(query);
        return View(report);
    }
   
}