using Dapper;
using ExpenseTracker.Dtos;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NToastNotify;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class BankTransactionController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;

    public BankTransactionController(IToastNotification toastNotification, IVoucherService voucherService,
        IBankService bankService)
    {
        _toastNotification = toastNotification;
        _voucherService = voucherService;
        _bankService = bankService;
    }

    [HttpGet]
    public IActionResult BankDepositandWithdraw()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BankDepositandWithdraw(BankTransactionVm vm)
    {
        try
        {
            using (var conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
            {
                using (var txn = conn.BeginTransaction())
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

                    await conn.CloseAsync();


                    var banktxn = await _bankService.RecordBankTransactionAsync(new BankTransactionDto
                    {
                        BankId = vm.BankId,
                        TxnDate = engtxndate,
                        Amount = vm.Amount,
                        Type = vm.Type,
                        Remarks = vm.Remarks,
                    });

                    var acctxn = await _voucherService.RecordTransactionAsync(new AccTransactionDto
                    {
                        TxnDate = engtxndate,
                        Amount = vm.Amount,
                        Type = vm.Type == "Deposit" ? "Bank Deposit" : "Bank Withdraw",
                        TypeId = banktxn.Id,
                        Remarks = vm.Remarks,
                        IsJv = false,
                        Details = new List<TransactionDetailDto>
                        {
                            new() { LedgerID = vm.BankId, IsDr = vm.Type == "Deposit", Amount = vm.Amount },
                            new() { LedgerID = -3, IsDr = vm.Type != "Deposit", Amount = vm.Amount },
                        }
                    });
                    await BankService.UpdateTransactionDuringBankTransaction(banktxn.Id, acctxn.Id);
                    await BankRemainingBalanceManager(vm.BankId);

                    _toastNotification.AddSuccessToastMessage("Bank " + vm.Type.ToLower() +
                                                              " completed with amount Rs. " + vm.Amount);
                    return View();
                }
            }
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage($"Issue recording {vm.Type.ToLower()}." + e.Message);
            return View();
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
                    // await ReverseService.ReverseBankTransactionByAccTranId(transactionid);

                    await _voucherService.ReverseTransactionAsync(transactionid);

                    _toastNotification.AddSuccessToastMessage("Bank " + type.ToLower() +
                                                              " reverse transaction completed");
                    return RedirectToAction("BankTransactionReport");
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    _toastNotification.AddErrorToastMessage("Issue reversing bank transaction: " + e.Message);
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