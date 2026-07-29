using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.ExtMethods;
using ExpenseTracker.Interface;
using ExpenseTracker.Providers.Interfaces;
using ExpenseTracker.Repository;
using ExpenseTracker.ViewModels;
using ExpenseTracker.ViewModels.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class BankController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IBankService _bankService;
    private readonly ILedgerService _ledgerService;
    private readonly IBankRepo _bankRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public BankController(IToastNotification toastNotification, IBankService bankService,
        IBankRepo bankRepo, ILedgerService ledgerService, ICurrentUserProvider currentUserProvider)
    {
        _toastNotification = toastNotification;
        _bankService = bankService;
        _bankRepo = bankRepo;
        _ledgerService = ledgerService;
        _currentUserProvider = currentUserProvider;
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
            var cu = await _currentUserProvider.GetCurrentUser();
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
                AccountOpenDate = vm.AccountOpenDate,
                LedgerId = lid.Id,
                RemainingBalance = 0,
                User = cu
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
        var res = await _bankRepo.FindOrThrowAsync(id);

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
        var res = await _bankRepo.GetBaseQueryable().FilterActiveStatus().ToListAsync();
        return View(res);
    }
}
