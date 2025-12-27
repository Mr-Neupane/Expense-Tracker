using System.Transactions;
using ExpenseTracker.Dtos;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.Manager;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class IncomeController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IVoucherService _voucherService;
    private readonly AccTransactionManager _transactionManager;
    private readonly IIncomeService _incomeService;
    private readonly IBankService _bankService;

    public IncomeController(IVoucherService voucherService, IToastNotification toastNotification,
        IIncomeService incomeService, IBankService bankService, AccTransactionManager transactionManager)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
        _incomeService = incomeService;
        _bankService = bankService;
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
            var engdate = await DateHelper.GetEnglishDate(vm.TxnDate);
            var income = new IncomeDto
            {
                Ledgerid = vm.IncomeLedger,
                FromLedgerid = vm.IncomeFrom,
                Amount = vm.Amount,
                Remarks = vm.Remarks,
                TxnDate = engdate.ToUniversalTime()
            };

            var accTransaction = new AccTransactionDto
            {
                TxnDate = engdate.ToUniversalTime(),
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
            return View();
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