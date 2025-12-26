using Dapper;
using ExpenseTracker.Dtos;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NToastNotify;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Controllers;

public class BankTransactionController : Controller
{
    private readonly IToastNotification _toastNotification;
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;

    public BankTransactionController(IToastNotification toastNotification, IVoucherService voucherService,
        IBankService bankService)
    {
        _toastNotification = toastNotification;
        _voucherService = voucherService;
        _bankService = bankService;
    }

    [HttpGet]
    public IActionResult BankDepositandWithdraw()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BankDepositandWithdraw(BankTransactionVm vm)
    {
        try
        {
            using (var conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
            {
                using (var txn = conn.BeginTransaction())
                {
                    var engtxndate = await DateHelper.GetEnglishDate(vm.TxnDate);
                    var frombankledgerid = await LedgerCode.GetBankLedgerId(vm.BankId);
                    var ledgerbalance = await BalanceProvider.GetLedgerBalance(frombankledgerid);
                    var banks = await _bankService.BankReportAsync();

                    var res = banks.FirstOrDefault(b => b.Id == vm.BankId);

                    if (res.RemainingBalance < vm.Amount)
                    {
                        _toastNotification.AddAlertToastMessage(
                            "Insufficient balance in bank for withdraw, Remaining bank balance is " + ledgerbalance +
                            ".");
                    }

                    await conn.CloseAsync();


                    var banktransaction = new BankTransactionDto
                    {
                        BankId = vm.BankId,
                        TxnDate = engtxndate,
                        Amount = vm.Amount,
                        Type = vm.Type,
                        Remarks = vm.Remarks,
                    };
                    var banktxn = await _bankService.RecordBankTransactionAsync(banktransaction);


                    var acctxn = await _voucherService.RecordTransactionAsync(new AccTransactionDto
                    {
                        TxnDate = engtxndate,
                        Amount = vm.Amount,
                        Type = vm.Type == "Deposit" ? "Bank Deposit" : "Bank Withdraw",
                        TypeId = banktxn.Id,
                        Remarks = vm.Remarks,
                        IsJv = false,
                        Details = new List<TransactionDetailDto>
                        {
                            new() { LedgerID = vm.BankId, IsDr = vm.Type == "Deposit", Amount = vm.Amount },
                            new() { LedgerID = -3, IsDr = vm.Type != "Deposit", Amount = vm.Amount },
                        }
                    });
                    await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(banktxn.Id, acctxn.Id);

                    await _bankService.UpdateRemainingBalanceInBankAsync(vm.BankId);

                    _toastNotification.AddSuccessToastMessage("Bank " + vm.Type.ToLower() +
                                                              " completed with amount Rs. " + vm.Amount);
                    return View();
                }
            }
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage($"Issue recording {vm.Type.ToLower()}." + e.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult?> BankTransactionReport()
    {
        var report = await BankService.GetBankTransactionReport();
        if (report != null || report.Any())
        {
            return View(report.ToList());
        }

        return RedirectToAction("BankDepositandWithdraw");
    }


    [HttpGet]
    public async Task<IActionResult> ReverseBankTransaction(int transactionid, string type, int bankId, decimal amount,
        int id)
    {
        try
        {
            var banks = await _bankService.BankReportAsync();

            var res = banks.FirstOrDefault(b => b.Id == bankId);

            if (res.RemainingBalance - amount < 0)
            {
                _toastNotification.AddErrorToastMessage("Not enough balance in bank to reverse transaction.");
            }
            else
            {
                await _bankService.ReverseBankTransactionAsync(id, transactionid);
                await _voucherService.ReverseTransactionAsync(transactionid);
                await _bankService.UpdateRemainingBalanceInBankAsync(bankId);

                _toastNotification.AddSuccessToastMessage("Bank " + type.ToLower() +
                                                          " reverse transaction completed");
            }

            return RedirectToAction("BankTransactionReport");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage("Issue reversing bank transaction: " + e.Message);
            return RedirectToAction("BankTransactionReport");
        }
    }
}