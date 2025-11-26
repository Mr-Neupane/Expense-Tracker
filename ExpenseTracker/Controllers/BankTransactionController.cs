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
                    var frombankledgerid = await LedgerCode.GetBankLedgerId(vm.BankId);
                    var ledgerbalance = await BalanceProvider.GetLedgerBalance(frombankledgerid);
                    if (vm.Type == "Withdraw" && vm.Amount > ledgerbalance)
                    {
                        TempData["AlertMessage"] =
                            "Insufficient balance in bank for withdraw, Remaining bank balance is " + ledgerbalance +
                            ".";
                        return RedirectToAction("BankDepositandWithdraw");
                    }

                    var banktran =
                        @"INSERT INTO bank.banktransactions ( bank_id,txn_date,amount,type,remarks,rec_date,rec_by_id,rec_status,status,transaction_id)
                                    values (@bank_id, @txn_date, @amount, @type, @remarks, @rec_date,@rec_by_id,@recs_tatus,@status,@transaction_id) returning id";
                    var banktxnid = await con.QuerySingleAsync<int>(banktran, new
                    {
                        bank_id = vm.BankId,
                        txn_date = vm.TxnDate,
                        amount = vm.Amount,
                        type = vm.Type,
                        remarks = vm.Remarks,
                        rec_date = DateTime.Now,
                        rec_by_id = -1,
                        recs_tatus = vm.RecStatus,
                        status = vm.Status,
                        transaction_id = 0,
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
        var conn = DapperConnectionProvider.GetConnection();
        var txnreport =
            await conn.QueryAsync(@"select b.id bankid,b.bankname,t.*,u.username
            from bank.banktransactions t
                join users u on u.id = t.rec_by_id
            join bank.bank b on b.id = bank_id where t.status=1");

        return View(txnreport);
    }

    [HttpPost]
    public static async Task OtherDeposits(int bankid, decimal amount, DateTime txndate, string remarks, string type,
        int tranid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var banktran =
                        @"INSERT INTO bank.banktransactions ( bank_id,txn_date,amount,type,remarks,rec_date,rec_by_id,rec_status,status,transaction_id)
                                    values (@bankid,@txndate,@amount,@type,@remarks,@recdate,@recbyid,@recstatus,@status,@transactionid)";
                    await conn.ExecuteAsync(banktran, new
                    {
                        bankid = bankid,
                        txndate = txndate,
                        amount = amount,
                        type = type,
                        remarks = remarks,
                        recdate = DateTime.Now,
                        recbyid = -1,
                        recstatus = 'A',
                        status = 1,
                        transactionid = tranid
                    });

                    await txn.CommitAsync();
                    await conn.CloseAsync();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await conn.CloseAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
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