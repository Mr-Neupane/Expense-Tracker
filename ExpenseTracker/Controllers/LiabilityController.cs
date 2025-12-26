using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TestApplication.Interface;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class LiabilityController : Controller
{
    private readonly ILiabilityService _liabilityService;
    private readonly IVoucherService _voucherService;


    public LiabilityController(IVoucherService voucherService, ILiabilityService liabilityService)
    {
        _liabilityService = liabilityService;
        _voucherService = voucherService;
    }

    // GET
    public IActionResult AddLiability()
    {
        return View();
    }

    [HttpPost]
    public async Task<RedirectToActionResult> AddLiability(LiabilityVm vm)
    {
        var engdate = DateTime.SpecifyKind(await DateHelper.GetEnglishDate(vm.TxnDate), DateTimeKind.Utc);
        var liability = _liabilityService.RecordLiabilityAsync(new LiabilityDto
        {
            LedgerId = vm.LiabilityLedger,
            TxnDate = engdate,
            Amount = vm.Amount,
            Remarks = vm.Remarks,
        });

        var bankid = await BankService.GetBankIdByLedgerId(vm.LiabilityFromLedger);

        var acctxn = await _voucherService.RecordTransactionAsync(new AccTransactionDto
        {
            TxnDate = engdate,
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
        });

        if (bankid != 0)
        {
            int banktransaction = await BankService.RecordBankTransaction(new BankTransactionVm
            {
                RecStatus = vm.RecStatus,
                Status = vm.Status,
                RecById = vm.RecById,
                BankId = bankid,
                TxnDate = vm.TxnDate,
                Amount = vm.Amount,
                Remarks = vm.Remarks,
                Type = "Deposit"
            });
            await BankService.UpdateTransactionDuringBankTransaction(banktransaction, acctxn.Id);
        }

        return RedirectToAction("AccountingTransaction", "Voucher");
    }

    [HttpGet]
    public async Task<IActionResult> LiabilityReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select e.*, voucher_no, username,t.id as transactionid
from accounting.liability e
         join accounting.transactions t on t.type_id = e.id
         join users u on e.rec_by_id = u.id
where t.type = 'Liability'
  and e.status = 1
  and t.status = 1";
        var report = await conn.QueryAsync(query);
        return View(report);
    }
}