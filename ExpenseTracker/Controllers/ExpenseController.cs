using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class ExpenseController : Controller
{
    private readonly IVoucherService _voucherService;
    private readonly IToastNotification _toastNotification;
    private readonly IBankService _bankService;
    private readonly ApplicationDbContext _context;

    public ExpenseController(IVoucherService voucherService, IToastNotification toastNotification,
        ApplicationDbContext context, IBankService bankService)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
        _context = context;
        _bankService = bankService;
    }

    [HttpGet]
    public IActionResult RecordExpense()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RecordExpense(ExpenseVm vm)
    {
        try
        {
            var engdate = await DateHelper.GetEnglishDate(vm.TxnDate);
            decimal frombalance = await BalanceProvider.GetLedgerBalance(vm.ExpenseFromLedger);
            if (vm.Amount > frombalance)
            {
                _toastNotification.AddAlertToastMessage("Insufficient balance on selected Ledger");
                return View();
            }

            var expenseid = _context.Expenses.AddRangeAsync(new Expense
            {
                LedgerId = vm.ExpenseLedger,
                DrAmount = vm.Amount,
                CrAmount = 0,
                TxnDate = engdate.ToUniversalTime(),
                RecDate = DateTime.Now.ToUniversalTime(),
                RecStatus = vm.RecStatus,
                Status = vm.Status,
                RecById = vm.RecById,
            });
            await _context.SaveChangesAsync();

            var accTrans = _voucherService.RecordTransactionAsync(
                new AccTransactionDto
                {
                    TxnDate = engdate,
                    Amount = vm.Amount,
                    Type = vm.Type,
                    TypeId = expenseid.Id,
                    Remarks = vm.Remarks,
                    IsJv = false,
                    Details = new List<TransactionDetailDto>()
                    {
                        new() { IsDr = true, Amount = vm.Amount, LedgerID = vm.ExpenseLedger },
                        new() { IsDr = false, Amount = vm.Amount, LedgerID = vm.ExpenseFromLedger }
                    }
                });

            var bankid = await BankService.GetBankIdByLedgerId(vm.ExpenseFromLedger);
            if (bankid != 0)
            {
                var bankTransaction = await _bankService.RecordBankTransactionAsync(new BankTransactionDto
                {
                    BankId = bankid,
                    TxnDate = engdate.ToUniversalTime(),
                    Amount = vm.Amount,
                    Type = "Withdraw",
                    Remarks = vm.Remarks
                });
                await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTransaction.Id,
                    accTrans.Id);
                await _bankService.UpdateRemainingBalanceInBankAsync(bankTransaction.BankId);
            }

            _toastNotification.AddSuccessToastMessage("Expense recorded successfully.");
            return RedirectToAction("ExpenseReport");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage("Expense could not be recorded." + e.Message);
            return View();
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
}