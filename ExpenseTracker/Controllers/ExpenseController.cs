using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Manager;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    private readonly DropdownProvider _dropdownProvider;


    public ExpenseController(IExpenseService expenseService, IToastNotification toastNotification,
        IBalanceProvider balanceProvider, AccTransactionManager accTransactionManager,
        DropdownProvider dropdownProvider)
    {
        _expenseService = expenseService;
        _toastNotification = toastNotification;
        _balanceProvider = balanceProvider;
        _accTransactionManager = accTransactionManager;
        _dropdownProvider = dropdownProvider;
    }

    [HttpGet]
    public IActionResult RecordExpense()
    {
        var expLedger = _dropdownProvider.GetExpenseLedgers();
        var cashAndBankLedger = _dropdownProvider.GetCashBankLedgers();
        var vm = new ExpenseVm()
        {
            ExpenseLedgers = new SelectList(expLedger, "Id", "Name"),
            CashAndBankLedgers = new SelectList(cashAndBankLedger, "Id", "Name")
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> RecordExpense(ExpenseVm vm)
    {
        try
        {
            var fromBalance = _balanceProvider.GetLedgerBalance(vm.ExpenseFromLedger);
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
            var cashAndBankLedger = _dropdownProvider.GetCashBankLedgers();
            var expenseLedger = _dropdownProvider.GetExpenseLedgers();
            var nvm = new ExpenseVm
            {
                ExpenseLedger = vm.ExpenseLedger,
                TxnDate = vm.TxnDate,
                ExpenseFromLedger = vm.ExpenseFromLedger,
                Type = vm.Type,
                Remarks = vm.Remarks,
                Amount = vm.Amount,
                ExpenseLedgers = new SelectList(expenseLedger, "Id", "Name"),
                CashAndBankLedgers = new SelectList(cashAndBankLedger, "Id", "Name")
            };
            _toastNotification.AddErrorToastMessage("Expense could not be recorded." + e.Message);
            return View(nvm);
        }
    }

    public async Task<IActionResult> ExpenseReport()
    {
        var report = await _expenseService.GetExpenseReportsAsync();
        return View(report);
    }
}