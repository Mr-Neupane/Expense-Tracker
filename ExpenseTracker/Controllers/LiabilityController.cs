using ExpenseTracker.Dtos;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.Manager;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class LiabilityController : Controller
{
    private readonly AccTransactionManager _accTransactionManager;
    private readonly ILiabilityService _liabilityService;
    private readonly IToastNotification _toastNotification;

    public LiabilityController(AccTransactionManager accTransactionManager, IToastNotification toastNotification,
        ILiabilityService liabilityService)
    {
        _accTransactionManager = accTransactionManager;
        _toastNotification = toastNotification;
        _liabilityService = liabilityService;
    }


// GET
    public IActionResult AddLiability()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddLiability(LiabilityVm vm)
    {
        try
        {
            // var engdate = DateTime.SpecifyKind(await DateHelper.GetEnglishDate(vm.TxnDate), DateTimeKind.Utc);
            var bankid = await BankService.GetBankIdByLedgerId(vm.LiabilityFromLedger);
            var liability = new LiabilityDto
            {
                LedgerId = vm.LiabilityLedger,
                BankId = bankid,
                TxnDate = vm.TxnDate,
                Amount = vm.Amount,
                Remarks = vm.Remarks,
            };
            var acctxn = new AccTransactionDto
            {
                TxnDate = vm.TxnDate,
                Amount = vm.Amount,
                Type = "Liability",
                TypeId = liability.Id,
                Remarks = vm.Remarks,
                IsJv = false,
                Details = new List<TransactionDetailDto>()
                {
                    new()
                    {
                        IsDr = true,
                        Amount = vm.Amount,
                        LedgerID = vm.LiabilityFromLedger
                    },
                    new()
                    {
                        IsDr = false,
                        Amount = vm.Amount,
                        LedgerID = vm.LiabilityLedger
                    }
                },
            };
            await _accTransactionManager.RecordLiabilityTransaction(liability, acctxn);
            _toastNotification.AddSuccessToastMessage("Liability recorded successfully");

            return RedirectToAction("LiabilityReport");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> LiabilityReport()
    {
        var report = await _liabilityService.GetAllLiabilityReportAsync();
        return View(report);
    }
}