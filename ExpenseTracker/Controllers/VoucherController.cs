using Dapper;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ExpenseTracker.Controllers;

public class VoucherController : Controller
{
    public static async Task<int> RecordAccountingTransaction(DateTime txndate, decimal dramount, decimal cramount,
        string type, int typeid, int fromledgerid, int toledgerid,
        string remarks)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var amount = dramount != 0 ? dramount : cramount;
                    var voucherno = await VoucherNumberProvider.GetVoucherNumber();
                    var query =
                        @"INSERT INTO accounting.transactions ( txndate, voucherno, amount, type, typeid, remarks, recstatus, recdate, status,
                                     recbyid)
                                     values ( @txndate, @voucherno, @amount, @type, @typeid, @remarks, @recstatus, @recdate, @status,
                                     @recbyid) returning id ";
                    var ins = await conn.QuerySingleAsync<int>(query, new
                    {
                        txndate, voucherno, amount, type, typeid, remarks, recstatus = 'A', recdate = DateTime.Now,
                        status = 1, recbyid = -1
                    });

                    var detailquery =
                        @"INSERT INTO accounting.transactiondetails (transactionid, ledgerid, dramount, cramount, drcr, recstatus, status,
                                           recbyid)
                                           values (@transactionid, @ledgerid, @dramount,@cramount, @drcr, @recstatus, @status,
                                           @recbyid)";

                    await conn.ExecuteAsync(detailquery, new
                    {
                        transactionid = ins,
                        ledgerid = fromledgerid,
                        dramount,
                        cramount,
                        drcr = dramount != 0 ? 'D' : 'C',
                        recstatus = 'A',
                        status = 1,
                        recbyid = -1
                    });


                    var detailquery2 =
                        @"INSERT INTO accounting.transactiondetails (transactionid, ledgerid, dramount, cramount, drcr, recstatus, status,
                                           recbyid)
                                           values (@transactionid, @ledgerid, @dramount,@cramount, @drcr, @recstatus, @status,
                                           @recbyid)";
                    await conn.ExecuteAsync(detailquery2, new
                    {
                        transactionid = ins,
                        ledgerid = toledgerid,
                        dramount = cramount,
                        cramount = dramount,
                        drcr = dramount != 0 ? 'C' : 'D',
                        recstatus = 'A',
                        status = 1,
                        recbyid = -1
                    });

                    await txn.CommitAsync();
                    return 0;
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

    public async Task<IActionResult> VoucherDetail(int transactionid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"
select ledgername,
       txndate,voucherno,
       dramount,
       cramount,
       drcr,
       username,t.id,
       t.type,
       t.typeid,
       remarks,
       code
from accounting.transactions t
         join accounting.transactiondetails td on t.id = td.transactionid
         join accounting.ledger l on l.id = ledgerid
         join users u on u.id = t.recbyid
where t.status = 1
  and td.status = 1
  and t.id = @transactionid";
        var report = await conn.QueryAsync(query, new { transactionid });
        return View(report.ToList());
    }

    public async Task<IActionResult> ReverseVoucher(int transactionid, int typeid, string type)
    {
        if (type == "Expense")
        {
            await ExpenseController.ReverseExpense(typeid, transactionid);
        }
        return RedirectToAction("VoucherDetail");
    }
}