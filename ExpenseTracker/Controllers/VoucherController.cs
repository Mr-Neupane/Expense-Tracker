using Dapper;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ExpenseTracker.Controllers;

public class VoucherController : Controller
{
    public static async Task<int> RecordAccountingTransaction(AccountingTxn model)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var amount = model.DrAmount != 0 ? model.DrAmount : model.CrAmount;
                    var voucherno = await VoucherNumberProvider.GetVoucherNumber();
                    var query =
                        @"INSERT INTO accounting.transactions ( txn_date, voucher_no, amount, type, type_id, remarks, rec_status, rec_date, status,
                                     rec_by_id)
                                     values ( @txndate, @voucherno, @amount, @type, @typeid, @remarks, @recstatus, @recdate, @status,
                                     @recbyid) returning id ";
                    var ins = await conn.QuerySingleAsync<int>(query, new
                    {
                        model.TxnDate, voucherno, amount, model.Type, model.TypeID, model.Remarks, recstatus = 'A',
                        recdate = DateTime.Now,
                        status = 1, recbyid = -1
                    });

                    var detailquery =
                        @"INSERT INTO accounting.transaction_details (transaction_id, ledger_id, dr_amount, cr_amount, dr_cr, rec_status, status,
                                           rec_by_id)
                                           values (@transactionid, @ledgerid, @dramount,@cramount, @drcr, @recstatus, @status,
                                           @recbyid)";

                    await conn.ExecuteAsync(detailquery, new
                    {
                        transactionid = ins,
                        ledgerid = model.FromLedgerID,
                        model.DrAmount,
                        model.CrAmount,
                        drcr = model.DrAmount != 0 ? 'D' : 'C',
                        recstatus = 'A',
                        status = 1,
                        recbyid = -1
                    });


                    var detailquery2 =
                        @"INSERT INTO accounting.transaction_details (transaction_id, ledger_id, dr_amount, cr_amount, dr_cr, rec_status, status,
                                           rec_by_id)
                                           values (@transactionid, @ledgerid, @dramount,@cramount, @drcr, @recstatus, @status,
                                           @recbyid)";
                    await conn.ExecuteAsync(detailquery2, new
                    {
                        transactionid = ins,
                        ledgerid = model.ToLedgerID,
                        dramount = model.CrAmount,
                        cramount = model.DrAmount,
                        drcr = model.DrAmount != 0 ? 'C' : 'D',
                        recstatus = 'A',
                        status = 1,
                        recbyid = -1
                    });

                    if (model.Type == "Income" || model.Type == "Expense")
                    {
                        if (model.Type == "Expense")
                        {
                            int bankid = await Validator.ValidateBankTransaction(model.ToLedgerID);
                            if (bankid != 0)
                            {
                                // await BankTransactionController.OtherDeposits(bankid, amount, txndate, remarks,
                                //     "Withdraw", ins);
                            }
                        }

                        if (model.Type == "Income")
                        {
                            int bankid = await Validator.ValidateBankTransaction(model.ToLedgerID);
                            if (bankid != 0)
                            {
                                // await BankTransactionController.OtherDeposits(bankid, amount, txndate, remarks,
                                //     "Deposit", ins);
                            }
                        }
                    }

                    await txn.CommitAsync();
                    return ins;
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

    public static async Task<int> GetInsertedAccountingId(AccountingTxn m)
    {
        int txnid = await RecordAccountingTransaction(m);
        return txnid;
    }

    public async Task<IActionResult> VoucherDetail(int transactionid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"
select ledgername,
       txn_date,voucher_no,
       dr_amount,
       cr_amount,
       dr_cr,
       username,t.id,
       t.type,
       t.type_id,
       remarks,
       code
from accounting.transactions t
         join accounting.transaction_details td on t.id = td.transaction_id
         join accounting.ledger l on l.id = ledger_id
         join users u on u.id = t.rec_by_id
where t.status = 1
  and td.status = 1
  and t.id = @transactionid";
        var report = await conn.QueryAsync(query, new { transactionid });
        return View(report.ToList());
    }

    public async Task<IActionResult> AccountingTransaction()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select t.id,voucher_no, txn_date, type, username, remarks, amount,status
from accounting.transactions t
         cross join lateral (select id
                             from accounting.transaction_details t2
                             where t.id = t2.transaction_id
                               and t2.rec_status = 'A'
                               and t2.status = 1
                             limit 1)d
         join users u on u.id = t.rec_by_id
where t.status = 1
  and t.rec_status = 'A';";

        var report = await conn.QueryAsync(query);
        var finalreport = report.ToList();
        if (finalreport.Any() || finalreport is not null)
        {
            return View(finalreport);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> ReverseVoucher(int transactionid, int typeid, string type)
    {
        if (type == "Expense")
        {
            await ExpenseController.ReverseExpense(typeid, transactionid);
        }
        else if (type == "Income")
        {
            await IncomeController.ReverseIncome(typeid, transactionid);
        }

        return RedirectToAction("AccountingTransaction");
    }
}