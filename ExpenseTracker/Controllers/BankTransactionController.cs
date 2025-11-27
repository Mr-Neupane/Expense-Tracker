using Dapper;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ExpenseTracker.Providers;

namespace ExpenseTracker.Controllers;

public class BankTransactionController : Controller
{
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
                    var frombankledgerid = await LedgerCode.GetBankLedgerId(vm.BankId);
                    var ledgerbalance = await BalanceProvider.GetLedgerBalance(frombankledgerid);
                    if (vm.Type == "Withdraw" && vm.Amount > ledgerbalance)
                    {
                        TempData["AlertMessage"] =
                            "Insufficient balance in bank for withdraw, Remaining bank balance is " + ledgerbalance +
                            ".";
                        return RedirectToAction("BankDepositandWithdraw");
                    }

                    int banktransactionid = await BankService.RecordBankTransaction(vm);

                    int transactionid = await VoucherController.GetInsertedAccountingId(new AccountingTxn
                    {
                        TxnDate = vm.TxnDate,
                        DrAmount = vm.Type == "Deposit" ? vm.Amount : 0,
                        CrAmount = vm.Type == "Withdraw" ? vm.Amount : 0,
                        Type = vm.Type == "Deposit" ? "Bank Deposit" : "Bank Withdraw",
                        TypeID = banktransactionid,
                        FromLedgerID = frombankledgerid,
                        ToLedgerID = -3,
                        Remarks = vm.Remarks,
                    });
                 await   BankService.UpdateTransactionDuringBankTransaction(banktransactionid, transactionid);
                    await txn.CommitAsync();
                    await BankRemainingBalanceManager(vm.BankId);
                    TempData["SuccessMessage"] =
                        "Bank " + vm.Type.ToLower() + " completed with amount Rs. " + vm.Amount;
                    return RedirectToAction("BankDepositandWithdraw");
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await con.CloseAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> BankTransactionReport()
    {
        var report = await BankService.GetBankTransactionReport();
        return View(report);
    }


    [HttpGet]
    public async Task<IActionResult> ReverseBankTransaction(int transactionid, string type, int bankId, decimal amount)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                int ledgerid = await conn.QueryFirstOrDefaultAsync<int>(
                    "select ledgerid from bank.bank where id = @bankid", new
                    {
                        bankId
                    });
                var bankbalance = await BalanceProvider.GetLedgerBalance(ledgerid);
                if (type == "Deposit" && amount > bankbalance)
                {
                    TempData["AlertMessage"] = "Can not perform reverse. Insufficient bank balance";
                    // TempData.Keep("AlertMessage");
                    return RedirectToAction("BankTransactionReport");
                }

                try
                {
                    await BankService.ReverseBankTransactionByAccTranID(transactionid);
                    // string revtype = type == "Deposit" ? "Bank Deposit" : "Bank Withdraw";
                    // var revbank = @"update bank.banktransactions set status = 2 where id = @id";
                    // await conn.ExecuteAsync(revbank, new { id });
                    //
                    // var revtxn =
                    //     @"update accounting.transactions set status=2 where typeid = @id and type = @revtype;";
                    //
                    // await conn.ExecuteAsync(revtxn, new { id, revtype });
                    //
                    // var reversetransactiondetail =
                    //     @"update accounting.transactiondetails set status=2 where transactionid in(select id from accounting.transactions t where t.typeid= @id and t.type= @revtype)";
                    // await conn.ExecuteAsync(reversetransactiondetail, new { id, revtype });
                    // await txn.CommitAsync();
                    // await BankRemainingBalanceManager(bankId);
                    // await conn.CloseAsync();


                    TempData["SuccessMessage"] = "Bank " + type.ToLower() + " reverse transaction completed";
                    // TempData.Keep("SuccessMessage");
                    return RedirectToAction("BankTransactionReport");
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    Console.WriteLine(e);
                    throw;
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
    where bank_id =1 group by bank_id
)  
update bank.bank b
set remainingbalance = amount
from bankd bd
where bd.bank_id = b.id;
", new { bankid });
    }
}