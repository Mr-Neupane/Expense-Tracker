using Dapper;
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
                    var frombankledgerid = await LedgerCode.GetBankLedgerId(vm);
                    var ledgerbalance = await BalanceProvider.GetLedgerBalance(frombankledgerid);
                    if (vm.Type == "Withdraw" && vm.Amount > ledgerbalance)
                    {
                        TempData["AlertMessage"] =
                            "Insufficient balance in bank for withdraw, Remaining bank balance is " + ledgerbalance +
                            ".";
                        return RedirectToAction("BankDepositandWithdraw");
                    }

                    var banktran =
                        @"INSERT INTO bank.banktransactions ( bankid, txndate, amount,type, remarks, recdate,recbyid, recstatus, status)
                                    values (@bankid,@txndate,@amount,@type,@remarks,@recdate,@recbyid,@recstatus,@status) returning id";
                    var banktxnid = await con.QuerySingleAsync<int>(banktran, new
                    {
                        bankid = vm.BankId,
                        txndate = vm.TxnDate,
                        amount = vm.Amount,
                        type = vm.Type,
                        remarks = vm.Remarks,
                        recdate = DateTime.Now,
                        recbyid = -1,
                        recstatus = vm.RecStatus,
                        status = vm.Status
                    });

                    string txntype = vm.Type == "Deposit" ? "Bank Deposit" : "Bank Withdraw";
                    decimal dramount = vm.Type == "Deposit" ? vm.Amount : 0;
                    decimal cramount = vm.Type == "Withdraw" ? vm.Amount : 0;
                    await VoucherController.RecordAccountingTransaction(vm.TxnDate, dramount, cramount, txntype,
                        banktxnid, frombankledgerid, -3,
                        vm.Remarks);
                    await txn.CommitAsync();
                    await BankRemainingBalanceManager(vm.BankId);
                    TempData["SuccessMessage"] =
                        "Bank " + vm.Type.ToLower() + " completed with amount Rs. " + vm.Amount;
                    return RedirectToAction("BankDepositandWithdraw");
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

    [HttpGet]
    public async Task<IActionResult> BankTransactionReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var txnreport =
            await conn.QueryAsync(@"select b.id bankid,b.bankname,t.*,u.username
            from bank.banktransactions t
                join users u on u.id = t.recbyid
            join bank.bank b on b.id = bankid where t.status=1");

        return View(txnreport);
    }

    [HttpGet]
    public async Task<IActionResult> ReverseBankTransaction(int id, string type, int bankId, decimal amount)
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
                    string revtype = type == "Deposit" ? "Bank Deposit" : "Bank Withdraw";
                    var revbank = @"update bank.banktransactions set status = 2 where id = @id";
                    await conn.ExecuteAsync(revbank, new { id });

                    var revtxn =
                        @"update accounting.transactions set status=2 where typeid = @id and type = @revtype;";

                    await conn.ExecuteAsync(revtxn, new { id, revtype });

                    var reversetransactiondetail =
                        @"update accounting.transactiondetails set status=2 where transactionid in(select id from accounting.transactions t where t.typeid= @id and t.type= @revtype)";
                    await conn.ExecuteAsync(reversetransactiondetail, new { id, revtype });
                    await txn.CommitAsync();
                    await BankRemainingBalanceManager(bankId);
                    await conn.CloseAsync();


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
    select sum(am)amount,bankid
    from (
        select sum(amount) am, bankid
        from bank.banktransactions t
        where type = 'Deposit'
          and status = 1
        group by bankid
        union
        select sum(amount) * -1 am, bankid
        from bank.banktransactions t
        where type = 'Withdraw'
          and status = 1
        group by bankid
    ) d
    where bankid = @bankid group by bankid
) 
update bank.bank b
set remainingbalance = amount
from bankd bd
where bd.bankid = b.id;
", new { bankid });
    }
}