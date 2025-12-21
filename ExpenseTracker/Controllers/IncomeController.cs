using Dapper;
using ExpenseTracker.Dtos;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NToastNotify;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class IncomeController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IVoucherService _voucherService;

    public IncomeController(IVoucherService voucherService, IToastNotification toastNotification)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
    }

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

                    var transaction = _voucherService.RecordTransactionAsync(new AccTransactionDto
                    {
                        TxnDate = engdate.ToUniversalTime(),
                        Amount = vm.Amount,
                        Type = vm.Type,
                        TypeId = incid,
                        Remarks = vm.Remarks,
                        IsJv = false,
                        Details = new List<TransactionDetailDto>
                        {
                            new() { LedgerID = vm.IncomeFrom, IsDr = true, Amount = vm.Amount },
                            new() { LedgerID = vm.IncomeLedger, IsDr = false, Amount = vm.Amount },
                        }
                    });

                    // var acctxnid = await VoucherController.GetInsertedAccountingId(new AccountingTxn
                    // {
                    //     TxnDate = engdate,
                    //     DrAmount = 0,
                    //     CrAmount = vm.Amount,
                    //     Type = vm.Type,
                    //     TypeID = incid,
                    //     FromLedgerID = vm.IncomeFrom,
                    //     ToLedgerID = vm.IncomeLedger,
                    //     Remarks = vm.Remarks
                    // });
                    int bankid = await BankService.GetBankIdByLedgerId(vm.IncomeFrom);
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
                        await BankService.UpdateTransactionDuringBankTransaction(banktranid, transaction.Id);
                    }

                    await txn.CommitAsync();
                    await conn.CloseAsync();
                    _toastNotification.AddSuccessToastMessage("Income recorded successfully.");
                    return View();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await conn.CloseAsync();
                    _toastNotification.AddErrorToastMessage(e.Message);
                    return View();
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
}