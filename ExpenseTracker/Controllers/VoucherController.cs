using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TestApplication.Interface;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class VoucherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;
    private readonly IIncomeService _incomeService;
    private readonly IToastNotification _toastNotification;

    public VoucherController(IVoucherService voucherService,
        IToastNotification toastNotification, ApplicationDbContext context, IIncomeService incomeService,
        IBankService bankService)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
        _context = context;
        _incomeService = incomeService;
        _bankService = bankService;
    }

    public async Task<IActionResult> VoucherDetail(int transactionid)
    {
        var res = await _voucherService.VoucherDetailAsync(transactionid);
        return View(res);
    }

    public async Task<IActionResult> AccountingTransaction()
    {
        var finalreport = await _voucherService.AccountingTransactionReportAsync();
        if (finalreport is not null)
        {
            return View(finalreport);
        }
        else
        {
            _toastNotification.AddWarningToastMessage("No Vouchers found");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult AddJv()
    {
        var model = new JournalVoucherVm();
        model.Entries.Add(new JournalEntryVm());
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddJv(JournalVoucherVm vm)
    {
        try
        {
            var txndate = await DateHelper.GetEnglishDate(vm.VoucherDate);
            var transaction = _voucherService.RecordTransactionAsync(
                new AccTransactionDto
                {
                    TxnDate = txndate.ToUniversalTime(),
                    Amount = vm.Entries.Sum(d => d.DrAmount),
                    Type = vm.Type,
                    TypeId = 0,
                    Remarks = vm.Remarks,
                    IsJv = true,
                    Details = vm.Entries.Select(e => new TransactionDetailDto
                    {
                        LedgerID = e.LedgerId, IsDr = e.DrAmount != 0,
                        Amount = e.CrAmount != 0 ? e.CrAmount : e.DrAmount,
                    }).ToList()
                });

            foreach (var data in vm.Entries)
            {
                var conn = DapperConnectionProvider.GetConnection();
                int? query = await conn.QueryFirstOrDefaultAsync<int>(
                    "select ledgerId from bank.bank where ledgerid = @ledgerid",
                    new { ledgerid = data.LedgerId });

                var bankledger = query ?? 0;

                var bankid = await BankService.GetBankIdByLedgerId(bankledger);
                var banktrans = vm.Entries.Where(e => e.LedgerId == bankledger)
                    .Select(e => new BankTransaction
                    {
                        BankId = bankid,
                        TxnDate = vm.VoucherDate.ToUniversalTime(),
                        Amount = e.DrAmount == 0 ? e.CrAmount : e.DrAmount,
                        Type = e.DrAmount != 0 ? "Deposit" : "Withdraw",
                        Remarks = vm.Narration,
                        RecDate = DateTime.UtcNow,
                        RecById = vm.RecById,
                        RecStatus = vm.RecStatus,
                        Status = vm.Status,
                        TransactionId = transaction.Id
                    }).ToList();
                await _context.BankTransaction.AddRangeAsync(banktrans);
                await _context.SaveChangesAsync();
            }

            _toastNotification.AddSuccessToastMessage("Journal voucher added successfully");
            return RedirectToAction("VoucherDetail", new { transactionid = transaction.Id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _toastNotification.AddErrorToastMessage("Issue creating voucher." + e.Message);
            return View();
        }
    }

    public async Task<IActionResult> ReverseVoucher(int transactionid, int typeid, string type)
    {
        switch (type)
        {
            case "Expense":
                await ReverseService.ReverseExpense(typeid, transactionid);
                break;
            case "Income":
                await _incomeService.ReverseIncomeAsync(typeid);
                await _voucherService.ReverseTransactionAsync(transactionid);
                break;
            case "Liability":
                await ReverseService.ReverseRecordedLiability(typeid, transactionid);
                break;
        }

        return RedirectToAction("AccountingTransaction");
    }
}