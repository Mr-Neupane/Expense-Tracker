using Dapper;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class IncomeController : Controller
{
    public IActionResult RecordIncome()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RecordIncome(IncomeVm vm)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var engdate = await DateHelper.GetEnglishDate(vm.TxnDate);
                    var query =
                        @"INSERT INTO accounting.income ( ledger_id, dr_amount, cr_amount, txn_date, rec_status, status, rec_date, rec_by_id)
                        values (@ledger_id, @dr_amount, @cr_amount, @txn_date, @rec_status, @status, @rec_date, @rec_by_id) returning id 
                        ";

                    int incid = await conn.QueryFirstAsync<int>(query, new
                    {
                        ledger_id = vm.IncomeLedger,
                        dr_amount = 0,
                        cr_amount = vm.Amount,
                        txn_date = engdate,
                        rec_status = vm.RecStatus,
                        status = vm.Status,
                        rec_date = DateTime.Now,
                        rec_by_id = -1
                    });

                    var acctxnid = await VoucherController.GetInsertedAccountingId(new AccountingTxn
                    {
                        TxnDate = engdate,
                        DrAmount = 0,
                        CrAmount = vm.Amount,
                        Type = vm.Type,
                        TypeID = incid,
                        FromLedgerID = vm.IncomeFrom,
                        ToLedgerID = vm.IncomeLedger,
                        Remarks = vm.Remarks
                    });
                    int bankid = await BankService.GetBankIdbyLedgerId(vm.IncomeFrom);
                    if (bankid != 0)
                    {
                        int banktranid = await BankService.RecordBankTransaction(new BankTransactionVm
                        {
                            RecStatus = vm.RecStatus,
                            Status = vm.Status,
                            RecById = vm.RecById,
                            BankId = bankid,
                            TxnDate = vm.TxnDate,
                            Amount = vm.Amount,
                            Remarks = vm.Remarks,
                            Type = "Deposit"
                        });
                        await BankService.UpdateTransactionDuringBankTransaction(banktranid, acctxnid);
                    }

                    await txn.CommitAsync();
                    await conn.CloseAsync();
                    TempData["SuccessMessage"] = "Income record successfully created";
                    return View("RecordIncome");
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

    public async Task<IActionResult> IncomeReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select e.*, voucher_no, username,t.id as transactionid
from accounting.income e
         join accounting.transactions t on t.type_id = e.id
         join users u on e.rec_by_id = u.id
where t.type = 'Income'
  and e.status = 1
  and t.status = 1";
        var report = await conn.QueryAsync(query);
        return View(report.ToList());
    }

    public static async Task ReverseIncome(int id, int transactionid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var mainupd = @"update accounting.income
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