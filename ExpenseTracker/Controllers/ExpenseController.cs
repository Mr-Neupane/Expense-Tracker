using Dapper;
using ExpenseTracker.Providers;
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
                        txn_date = vm.TxnDate,
                        rec_status = vm.RecStatus,
                        status = 1,
                        rec_date = DateTime.Now,
                        rec_by_id = -1
                    });
                    await VoucherController.RecordAccountingTransaction(vm.TxnDate, vm.Amount, 0, vm.Type, expinsid,
                        vm.ExpenseLedger, vm.ExpenseFromLedger, vm.Remarks);
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

        return View();
    }
}