using ExpenseTracker.Dtos;
using ExpenseTracker.Manager;
using ExpenseTracker.Providers;
using ExpenseTracker.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using ExpenseTracker.Repository;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Controllers;

public class IncomeController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly AccTransactionManager _transactionManager;
    private readonly DropdownProvider _dropdownProvider;
    private readonly IIncomeRepo _incomeRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public IncomeController(IToastNotification toastNotification, AccTransactionManager transactionManager, DropdownProvider dropdownProvider, IIncomeRepo incomeRepo, ICurrentUserProvider currentUserProvider)
    {
        _toastNotification = toastNotification;
        _transactionManager = transactionManager;
        _dropdownProvider = dropdownProvider;
        _incomeRepo = incomeRepo;
        _currentUserProvider = currentUserProvider;
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
            var cu = await _currentUserProvider.GetCurrentUser();
            var income = new IncomeDto
            {
                Ledgerid = vm.IncomeLedger,
                FromLedgerid = vm.IncomeFrom,
                Amount = vm.Amount,
                Remarks = vm.Remarks,
                TxnDate = vm.TxnDate,
                User= cu
            };

            var accTransaction = new AccTransactionDto
            {
                TxnDate = vm.TxnDate,
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
        var res = await _incomeRepo.GetIncomeReportAsync();
        if (res.Count == 0)
        {
            _toastNotification.AddAlertToastMessage("No matching data found.");
        }

        return View(res);
    }
}