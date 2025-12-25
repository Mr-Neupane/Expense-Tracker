using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class ExpenseController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly IVoucherService _voucherService;
    private readonly IToastNotification _toastNotification;
    private readonly IBankService _bankService;
    private readonly ApplicationDbContext _context;

    public ExpenseController(IVoucherService voucherService, IToastNotification toastNotification,
        ApplicationDbContext context, IBankService bankService, IExpenseService expenseService)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
        _context = context;
        _bankService = bankService;
        _expenseService = expenseService;
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

            var expense = await _expenseService.RecordExpenseAsync(new NewExpenseDto
            {
                LedgerId = vm.ExpenseLedger,
                Amount = vm.Amount,
                TxnDate = engdate,
            });

            var accTrans = _voucherService.RecordTransactionAsync(
                new AccTransactionDto
                {
                    TxnDate = engdate,
                    Amount = vm.Amount,
                    Type = vm.Type,
                    TypeId = expense.Id,
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
        var report =await _expenseService.GetExpenseReportsAsync();
        return View(report);
    }
}