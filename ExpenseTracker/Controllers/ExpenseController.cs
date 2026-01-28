using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Manager;
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
    private readonly IToastNotification _toastNotification;
    private readonly IBalanceProvider _balanceProvider;
    private readonly AccTransactionManager _accTransactionManager;


    public ExpenseController(IExpenseService expenseService, IToastNotification toastNotification,
        IBalanceProvider balanceProvider, AccTransactionManager accTransactionManager)
    {
        _expenseService = expenseService;
        _toastNotification = toastNotification;
        _balanceProvider = balanceProvider;
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
            var fromBalance = await _balanceProvider.GetLedgerBalance(vm.ExpenseFromLedger);
            if (vm.Amount > fromBalance)
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