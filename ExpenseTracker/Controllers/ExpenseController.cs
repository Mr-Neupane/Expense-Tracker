using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.Manager;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class ExpenseController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly IVoucherService _voucherService;
    private readonly IToastNotification _toastNotification;
    private readonly IBankService _bankService;
    private readonly AccTransactionManager _accTransactionManager;

    public ExpenseController(IVoucherService voucherService, IToastNotification toastNotification,
        IBankService bankService, IExpenseService expenseService, AccTransactionManager accTransactionManager)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
        _bankService = bankService;
        _expenseService = expenseService;
        _accTransactionManager = accTransactionManager;
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
            // var engdate = await DateHelper.GetEnglishDate(vm.TxnDate);
            decimal frombalance = await BalanceProvider.GetLedgerBalance(vm.ExpenseFromLedger);
            if (vm.Amount > frombalance)
            {
                _toastNotification.AddAlertToastMessage("Insufficient balance on selected Ledger");
                return View();
            }

            var expense = new NewExpenseDto
            {
                LedgerId = vm.ExpenseLedger,
                FromLedgerId = vm.ExpenseFromLedger,
                Amount = vm.Amount,
                TxnDate = vm.TxnDate,
            };

            var accTrans =
                new AccTransactionDto
                {
                    TxnDate = vm.TxnDate,
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
                };

            await _accTransactionManager.RecordExpenseTransaction(expense, accTrans);

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
        var report = await _expenseService.GetExpenseReportsAsync();
        return View(report);
    }
}