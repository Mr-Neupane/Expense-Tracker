using Dapper;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ExpenseTracker.Services;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class BankTransactionController : Controller
{
    private readonly IToastNotification _toastNotification;

    public BankTransactionController(IToastNotification toastNotification)
    {
        _toastNotification = toastNotification;
    }

    [HttpGet]
    public IActionResult BankDepositandWithdraw()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BankDepositandWithdraw(BankTransactionVm vm)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    var engtxndate = await DateHelper.GetEnglishDate(vm.TxnDate);
                    var frombankledgerid = await LedgerCode.GetBankLedgerId(vm.BankId);
                    var ledgerbalance = await BalanceProvider.GetLedgerBalance(frombankledgerid);
                    if (vm.Type == "Withdraw" && vm.Amount > ledgerbalance)
                    {
                        _toastNotification.AddAlertToastMessage(
                            "Insufficient balance in bank for withdraw, Remaining bank balance is " + ledgerbalance +
                            ".");
                        return RedirectToAction("BankDepositandWithdraw");
                    }

                    int banktransactionid = await BankService.RecordBankTransaction(vm);

                    int transactionid = await VoucherController.GetInsertedAccountingId(new AccountingTxn
                    {
                        TxnDate = engtxndate,
                        DrAmount = vm.Type == "Deposit" ? vm.Amount : 0,
                        CrAmount = vm.Type == "Withdraw" ? vm.Amount : 0,
                        Type = vm.Type == "Deposit" ? "Bank Deposit" : "Bank Withdraw",
                        TypeID = banktransactionid,
                        FromLedgerID = frombankledgerid,
                        ToLedgerID = -3,
                        Remarks = vm.Remarks,
                    });
                    await BankService.UpdateTransactionDuringBankTransaction(banktransactionid, transactionid);
                    await txn.CommitAsync();
                    await con.CloseAsync();
                    await BankRemainingBalanceManager(vm.BankId);

                    _toastNotification.AddSuccessToastMessage("Bank " + vm.Type.ToLower() +
                                                              " completed with amount Rs. " + vm.Amount);
                    return View();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await con.CloseAsync();
                    _toastNotification.AddErrorToastMessage($"Issue recording {vm.Type.ToLower()}." + e.Message);
                    return View();
                }
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult?> BankTransactionReport()
    {
        var report = await BankService.GetBankTransactionReport();
        if (report != null || report.Any())
        {
            return View(report.ToList());
        }

        return RedirectToAction("BankDepositandWithdraw");
    }


    [HttpGet]
    public async Task<IActionResult> ReverseBankTransaction(int transactionid, string type, int bankId, decimal amount,
        int id)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    await ReverseService.ReverseBankTransactionByAccTranId(transactionid);
                    _toastNotification.AddSuccessToastMessage("Bank " + type.ToLower() + " reverse transaction completed");
                    return RedirectToAction("BankTransactionReport");
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    _toastNotification.AddErrorToastMessage("Issuee reversing bank transaction: " + e.Message);
                    return RedirectToAction("BankTransactionReport");
                }
            }
        }
    }

    [HttpPost]
    public static async Task BankRemainingBalanceManager(int bankid)
    {
        var con = DapperConnectionProvider.GetConnection();

        await con.ExecuteAsync(@"
;with bankd as (
    select sum(am)amount,bank_id
    from (
        select sum(amount) am, bank_id
        from bank.banktransactions t
        where type = 'Deposit'
          and status = 1
        group by bank_id
        union
        select sum(amount) * -1 am, bank_id
        from bank.banktransactions t
        where type = 'Withdraw'
          and status = 1
        group by bank_id
    ) d
    group by bank_id
)  
update bank.bank b
set remainingbalance = amount
from bankd bd
where bd.bank_id = b.id;
", new { bankid });
    }
}