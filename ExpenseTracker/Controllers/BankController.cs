using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.ViewModels;
using ExpenseTracker.ViewModels.Interface;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class BankController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IBankService _bankService;
    private readonly ILedgerService _ledgerService;
    private readonly IBankRepo _bankGenericRepo;

    public BankController(IToastNotification toastNotification, IBankService bankService,
        IBankRepo bankGenericRepo, ILedgerService ledgerService)
    {
        _toastNotification = toastNotification;
        _bankService = bankService;
        _bankGenericRepo = bankGenericRepo;
        _ledgerService = ledgerService;
    }

    [HttpGet]
    public IActionResult CreateBank()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateBank(BankVm vm)
    {
        try
        {

            var lid = await _ledgerService.AddLedgerAsync(new LedgerDto
            {
                Name = vm.BankName,
                ParentId = null,
                SubParentId = LedgerConstants.BankAccount,
            });

            await _bankService.AddBankAsync(new BankDto
            {
                BankName = vm.BankName,
                AccountNumber = vm.AccountNumber,
                BankContact = vm.BankContact,
                BankAddress = vm.BankAddress,
                AccountOpenDate = vm.AccountOpenDate.ToUniversalTime(),
                LedgerId = lid.Id,
                RemainingBalance = 0
            });

            _toastNotification.AddSuccessToastMessage($"{vm.BankName} created");

            return RedirectToAction("BankReport");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage("Error creating bank." + e.Message);
            return View();
        }
    }


    [HttpGet]
    public async Task<IActionResult> EditBank(int id)
    {
        var res = await _bankGenericRepo.FindOrThrowAsync(id);

        var editBankDetail = new BankDto
        {
            Id = res.Id,
            BankName = res.BankName,
            AccountNumber = res.AccountNumber,
            BankContact = res.BankContactNumber,
            BankAddress = res.BankAddress,
            AccountOpenDate = res.AccountOpenDate,
            LedgerId = res.LedgerId,
            RemainingBalance = res.RemainingBalance
        };

        return View(editBankDetail);
    }

    [HttpPost]
    public async Task<IActionResult> EditBank(BankDto dto)
    {
        await _bankService.EditBankAsync(dto);
        _toastNotification.AddSuccessToastMessage("Bank edited successfully");
        return RedirectToAction("BankReport");
    }

    [HttpGet]
    public async Task<IActionResult> BankReport()
    {
        var res = await _bankService.BankReportAsync();
        return View(res);
    }
}
