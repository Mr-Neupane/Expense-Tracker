using System.Transactions;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Manager;
using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
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
    private readonly IBankService _bankService;


    public VoucherController(ApplicationDbContext context, IVoucherService voucherService,
        ReverseTransactionManager reverseTransactionManager, IToastNotification toastNotification,
        DropdownProvider dropdownProvider, IBankService bankService)
    {
        _context = context;
        _voucherService = voucherService;
        _reverseTransactionManager = reverseTransactionManager;
        _toastNotification = toastNotification;
        _dropdownProvider = dropdownProvider;
        _bankService = bankService;
    }

    public async Task<IActionResult> VoucherDetail(int transactionId)
    {
        var res = await _voucherService.VoucherDetailAsync(transactionId);
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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
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

                foreach (var d in vm.Entries)
                {
                    var bank = _context.Banks.SingleOrDefault(x => x.LedgerId == d.LedgerId);

                    if (bank != null)
                    {
                        var bankTransDto =
                            new BankTransactionDto
                            {
                                BankId = bank.Id,
                                TxnDate = vm.VoucherDate.ToUniversalTime(),
                                Amount = d.DrAmount == 0 ? d.CrAmount : d.DrAmount,
                                Type = d.DrAmount != 0 ? "Deposit" : "Withdraw",
                                Remarks = vm.Narration,
                            };
                        var bankTxn = await _bankService.RecordBankTransactionAsync(bankTransDto);
                        await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTxn.Id,
                            transaction.Id);
                    }
                }

                scope.Complete();

                _toastNotification.AddSuccessToastMessage("Journal voucher added successfully");
                return RedirectToAction("VoucherDetail", new { transactionid = transaction.Id });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _toastNotification.AddErrorToastMessage("Issue creating voucher." + e.Message);
            return View();
        }
    }

    public async Task<IActionResult> ReverseVoucher(int transactionId, int typeId, string type)
    {
        try
        {
            {
                switch (type)
                {
                    case "Expense":
                        await _reverseTransactionManager.ReverseExpenseTransaction(typeId, transactionId);

                        break;
                    case "Income":
                        await _reverseTransactionManager.ReverseIncomeTransaction(typeId, transactionId);
                        break;
                    case "Liability":
                        await _reverseTransactionManager.ReverseLiabilityTransaction(typeId, transactionId);
                        break;
                    case "Journal Voucher":
                        await _reverseTransactionManager.ReverseJournalTransaction(transactionId);
                        break;
                }

                _toastNotification.AddAlertToastMessage("Voucher reversed successfully");
                return RedirectToAction("AccountingTransaction");
            }
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            return RedirectToAction("AccountingTransaction");
        }
    }
}