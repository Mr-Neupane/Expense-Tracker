using ExpenseTracker.Dtos;
using ExpenseTracker.Manager;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class IncomeController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly AccTransactionManager _transactionManager;
    private readonly IIncomeService _incomeService;
    private readonly DropdownProvider _dropdownProvider;

    public IncomeController(IToastNotification toastNotification,
        IIncomeService incomeService, AccTransactionManager transactionManager, DropdownProvider dropdownProvider)
    {
        _toastNotification = toastNotification;
        _incomeService = incomeService;
        _transactionManager = transactionManager;
        _dropdownProvider = dropdownProvider;
    }

    public IActionResult RecordIncome()
    {
        var incomeLedger = _dropdownProvider.GetIncomeLedgers();
        var cashAndBank = _dropdownProvider.GetCashBankLedgers();
        var vm = new IncomeVm
        {
            IncomeLedgerList = new SelectList(incomeLedger, "Id", "Name"),
            CashAndBankLedger = new SelectList(cashAndBank, "Id", "Name"),
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> RecordIncome(IncomeVm vm)
    {
        try
        {
            var income = new IncomeDto
            {
                Ledgerid = vm.IncomeLedger,
                FromLedgerid = vm.IncomeFrom,
                Amount = vm.Amount,
                Remarks = vm.Remarks,
                TxnDate = vm.TxnDate.ToUniversalTime()
            };

            var accTransaction = new AccTransactionDto
            {
                TxnDate = vm.TxnDate.ToUniversalTime(),
                Amount = vm.Amount,
                Type = vm.Type,
                TypeId = income.Id,
                Remarks = vm.Remarks,
                IsJv = false,
                Details = new List<TransactionDetailDto>
                {
                    new() { LedgerID = vm.IncomeFrom, IsDr = true, Amount = vm.Amount },
                    new() { LedgerID = vm.IncomeLedger, IsDr = false, Amount = vm.Amount },
                }
            };

            await _transactionManager.RecordIncomeTransaction(income, accTransaction);


            _toastNotification.AddSuccessToastMessage("Income recorded successfully.");
            return RedirectToAction("IncomeReport");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            var incomeLedger = _dropdownProvider.GetIncomeLedgers();
            var cashAndBank = _dropdownProvider.GetCashBankLedgers();
            var rvm = new IncomeVm
            {
                IncomeLedger = vm.IncomeLedger,
                Amount = vm.Amount,
                Type = vm.Type,
                TxnDate = vm.TxnDate,
                IncomeFrom = vm.IncomeFrom,
                IncomeLedgerList = new SelectList(incomeLedger,
                    "Id",
                    "Name"),
                CashAndBankLedger = new SelectList(cashAndBank,
                    "Id",
                    "Name"),
            };
            return View(rvm);
        }
    }

    public async Task<IActionResult> IncomeReport()
    {
        var res = await _incomeService.GetIncomeReportAsync();
        if (res.Count == 0)
        {
            _toastNotification.AddAlertToastMessage("No matching data found.");
        }

        return View(res);
    }
}