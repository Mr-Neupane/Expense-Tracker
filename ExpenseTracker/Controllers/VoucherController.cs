using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TestApplication.Manager;
using TestApplication.ViewModels;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class VoucherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVoucherService _voucherService;
    private readonly ReverseTransactionManager _reverseTransactionManager;
    private readonly IToastNotification _toastNotification;
    private readonly DropdownProvider _dropdownProvider;
    private readonly IProvider _provider;

    public VoucherController(IVoucherService voucherService,
        IToastNotification toastNotification, ApplicationDbContext context,
        ReverseTransactionManager reverseTransactionManager, DropdownProvider dropdownProvider, IProvider provider)
    {
        _voucherService = voucherService;
        _toastNotification = toastNotification;
        _context = context;
        _reverseTransactionManager = reverseTransactionManager;
        _dropdownProvider = dropdownProvider;
        _provider = provider;
    }

    public async Task<IActionResult> VoucherDetail(int transactionid)
    {
        var res = await _voucherService.VoucherDetailAsync(transactionid);
        return View(res);
    }

    [HttpGet]
    public async Task<IActionResult> AccountingTransaction()
    {
        var type = await _dropdownProvider.GetTransactionTypeAsync();
        var transactions = _context.AccountingTransaction.ToList();

        var model =
            new AccountingTxnVm
            {
                TransactionsSelectList = new SelectList(
                    type
                )
            };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AccountingTransaction(AccountingTxnVm vm)
    {
        var type = await _dropdownProvider.GetTransactionTypeAsync();
        var filter = new TransactionReportDto
        {
            DateFrom = vm.DateFrom.ToUniversalTime(),
            DateTo = vm.DateTo.ToUniversalTime(),
            Type = vm.TxnType,
            Status = vm.Status,
        };
        var finalreport = await _voucherService.AccountingTransactionReportAsync(filter);
        var res = new AccountingTxnVm
        {
            DateFrom = vm.DateFrom.ToUniversalTime(),
            DateTo = vm.DateTo.ToUniversalTime(),
            TxnType = vm.TxnType,
            Status = vm.Status,
            AccountingTransactions = finalreport.Select(r => new AccountingTransactionReportDto
                {
                    TxnDate = r.TxnDate,
                    VoucherNo = r.VoucherNo,
                    Remarks = r.Remarks,
                    Type = r.Type,
                    Username = r.Username,
                    Amount = r.Amount,
                    Status = r.Status,
                    TransactionId = r.TransactionId,
                })
                .ToList(),
            TransactionsSelectList = new SelectList(type)
        };
        if (finalreport.Count <= 0)
        {
            _toastNotification.AddAlertToastMessage("No Vouchers found");
        }

        return View(res);
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
            var transaction = await _voucherService.RecordTransactionAsync(
                new AccTransactionDto
                {
                    TxnDate = vm.VoucherDate.ToUniversalTime(),
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
                var bankLedger = await (from b in _context.Banks where b.LedgerId == data.LedgerId select b)
                    .FirstOrDefaultAsync();
                if (bankLedger != null)
                {
                    var bankTrans = vm.Entries.Where(e => e.LedgerId == bankLedger.LedgerId)
                        .Select(e => new BankTransaction
                        {
                            BankId = bankLedger.Id,
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
                    await _context.BankTransaction.AddRangeAsync(bankTrans);
                    await _context.SaveChangesAsync();
                }
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
                await _reverseTransactionManager.ReverseExpenseTransaction(typeid, transactionid);

                break;
            case "Income":
                await _reverseTransactionManager.ReverseIncomeTransaction(typeid, transactionid);
                break;
            case "Liability":
                await _reverseTransactionManager.ReverseLiabilityTransaction(typeid, transactionid);
                break;
            case "Journal Voucher":
                await _reverseTransactionManager.ReverseJournalTransaction(transactionid);
                break;
        }

        _toastNotification.AddAlertToastMessage("Voucher reversed successfully");
        return RedirectToAction("AccountingTransaction");
    }
}