using Dapper;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TestApplication.ViewModels;

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

                    var accountingTransaction =
                        @"INSERT INTO accounting.transactions ( txndate, amount, type, typeid, remarks, recstatus, recdate, status, recbyid)
                                   values (@txndate,@amount,@type,@typeid,@remarks,@recstatus,@recdate,@status,@recbyid) returning id";
                    var acctxnid = await con.QuerySingleAsync<int>(accountingTransaction, new
                    {
                        txndate = vm.TxnDate,
                        amount = vm.Amount,
                        Type = txntype,
                        typeid = banktxnid,
                        remarks = vm.Remarks,
                        recstatus = vm.RecStatus,
                        recdate = DateTime.Now,
                        status = vm.Status,
                        recbyid = -1
                    });
                    var bankledgerid = @"select ";
                    var transactionDetail = @"
INSERT INTO accounting.transactiondetails ( transactionid,ledgerid, dramount, cramount, drcr, recstatus, status, recbyid)
values (@transactionid,@ledgerid,@dramount,@cramount,@drcr,@recstatus,@status,@recbyid)
";
                    await con.ExecuteAsync(transactionDetail, new
                    {
                        transactionid = acctxnid,
                        // ledgerid =,
                        dramount = vm.Type=="Deposit"?vm.Amount:0,
                        cramount = vm.Type=="Withdraw"?vm.Amount:0,
                        drcr = vm.Type=="Deposit"?"D":"C",
                        recstatus = vm.RecStatus,
                        status = vm.Status,
                        recbyid = -1
                    });

                    await con.ExecuteAsync(transactionDetail, new
                    {
                        transactionid = acctxnid,
                        dramount = vm.Type == "Deposit" ? vm.Amount : 0,
                        cramount = vm.Type == "Withdraw" ? vm.Amount : 0,
                        drcr = vm.Type == "Deposit" ? vm.Amount : 0,
                        recstatus = vm.RecStatus,
                        status = vm.Status,
                        recbyid = -1

                    });

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
", new { bankid = vm.BankId });
                    await txn.CommitAsync();

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
    public JsonResult GetBanks()
    {
        var dbConnection = DapperConnectionProvider.GetConnection();
        string query = "SELECT id, bankname FROM bank.bank";
        var banks = dbConnection.Query(query).Select(b => new
        {
            Id = b.id,
            bankname = b.bankname
        }).ToList();
        return Json(banks);
    }
}