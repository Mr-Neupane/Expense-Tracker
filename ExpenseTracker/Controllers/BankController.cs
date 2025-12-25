using Dapper;
using ExpenseTracker.Dtos;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Providers;
using Npgsql;
using NToastNotify;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class BankController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IBankService _bankService;

    public BankController(IToastNotification toastNotification, IBankService bankService)
    {
        _toastNotification = toastNotification;
        _bankService = bankService;
    }

    [HttpGet]
    public IActionResult CreateBank()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateBank(BankVm vm)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    int subparentId = -2;

                    DateTime accountopendate = await DateHelper.GetEnglishDate(vm.AccountOpenDate);
                    int lid = await LedgerController.NewLedger(new LedgerVm
                    {
                        Id = vm.Id,
                        SubParentId = subparentId,
                        ParentId = 0,
                        LedgerName = vm.BankName
                    });

                    await _bankService.AddBankAsync(new BankDto
                    {
                        BankName = vm.BankName,
                        AccountNumber = vm.AccountNumber,
                        BankContact = vm.BankContact,
                        BankAddress = vm.BankAddress,
                        AccountOpenDate = accountopendate.ToUniversalTime(),
                        LedgerId = lid,
                        RemainingBalance = 0
                    });

                   _toastNotification.AddSuccessToastMessage($"{vm.BankName} created");

                    return RedirectToAction("BankReport");
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    _toastNotification.AddErrorToastMessage("Error creating bank." + e.Message);
                    return View();
                }
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> BankReport()
    {
      var res=  await _bankService.BankReportAsync();
        return View(res);
    }
}