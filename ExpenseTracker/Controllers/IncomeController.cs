using ExpenseTracker.Dtos;
using ExpenseTracker.Manager;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class IncomeController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly AccTransactionManager _transactionManager;
    private readonly IIncomeService _incomeService;

    public IncomeController(IToastNotification toastNotification,
        IIncomeService incomeService, AccTransactionManager transactionManager)
    {
        _toastNotification = toastNotification;
        _incomeService = incomeService;
        _transactionManager = transactionManager;
    }

    public IActionResult RecordIncome()
    {
        return View();
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
            return View();
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