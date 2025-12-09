using Dapper;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class ExpenseController : Controller
{
    [HttpGet]
    public IActionResult RecordExpense()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RecordExpense(ExpenseVm vm)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var engdate = await DateHelper.GetEnglishDate(vm.TxnDate);
                    decimal frombalance = await BalanceProvider.GetLedgerBalance(vm.ExpenseFromLedger);
                    if (vm.Amount > frombalance)
                    {
                        TempData["AlertMessage"] = "Insufficient balance on selected Ledger";
                        return RedirectToAction("RecordExpense");
                    }

                    var query =
                        @"INSERT INTO accounting.expenses ( ledger_id, dr_amount, cr_amount, txn_date, rec_status, status, rec_date, rec_by_id)
                    values (@ledger_id ,@dr_amount , @cr_amount , @txn_date , @rec_status , @status, @rec_date , @rec_by_id) returning id";

                    int expinsid = await conn.QueryFirstAsync<int>(query, new
                    {
                        ledger_id = vm.ExpenseFromLedger,
                        dr_amount = vm.Amount,
                        cr_amount = 0,
                        txn_date = engdate,
                        rec_status = vm.RecStatus,
                        status = 1,
                        rec_date = DateTime.Now,
                        rec_by_id = -1
                    });
                    var acctxnid = await VoucherController.GetInsertedAccountingId(new AccountingTxn
                    {
                        TxnDate = engdate,
                        DrAmount = vm.Amount,
                        CrAmount = 0,
                        Type = vm.Type,
                        TypeID = expinsid,
                        FromLedgerID = vm.ExpenseLedger,
                        ToLedgerID = vm.ExpenseFromLedger,
                        Remarks = vm.Remarks
                    });
                    var bankid = await BankService.GetBankIdbyLedgerId(vm.ExpenseFromLedger);
                    if (bankid != 0)
                    {
                        var banktranid = await BankService.RecordBankTransaction(new BankTransactionVm
                        {
                            RecStatus = vm.RecStatus,
                            Status = vm.Status,
                            RecById = vm.RecById,
                            BankId = 1,
                            TxnDate = vm.TxnDate,
                            Amount = vm.Amount,
                            Remarks = vm.Remarks,
                            Type = "Withdraw",
                        });
                        await BankService.UpdateTransactionDuringBankTransaction(banktranid, acctxnid);
                    }


                    await txn.CommitAsync();
                    await conn.CloseAsync();
                    TempData["SuccessMessage"] = "Expense record successfully created";
                    return RedirectToAction("ExpenseReport");
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await conn.CloseAsync();
                    TempData["ErrorMessage"] = e.Message;
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public async Task<IActionResult> ExpenseReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select e.*, voucher_no, username,t.id as transactionid
from accounting.expenses e
         join accounting.transactions t on t.type_id = e.id
         join users u on e.rec_by_id = u.id
where t.type = 'Expense'
  and e.status = 1
  and t.status = 1";
        var report = await conn.QueryAsync(query);
        return View(report);
    }

    public static async Task ReverseExpense(int id, int transactionid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var mainupd = @"update accounting.expenses
                    set status=2
                    where id = @id;";

                    await conn.ExecuteAsync(mainupd, new { id });

                    var acctran = @"update accounting.transactions
                    set status=2 where 
                   id= @transactionid ;";

                    await conn.ExecuteAsync(acctran, new { transactionid });

                    var detail = @"update accounting.transaction_details
                    set status=2
                    where transaction_id= @transactionid ;";

                    await conn.ExecuteAsync(detail, new { transactionid });

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
}