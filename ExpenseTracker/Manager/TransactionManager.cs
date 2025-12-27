using System.Transactions;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using TestApplication.Interface;
using TestApplication.ViewModels.Interface;

namespace TestApplication.Manager;

public class TransactionManager
{
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;
    private readonly IIncomeService _incomeService;
    private readonly IExpenseService _expenseService;

    public TransactionManager(IVoucherService voucherService, IBankService bankService, IIncomeService incomeService,
        IExpenseService expenseService)
    {
        _voucherService = voucherService;
        _bankService = bankService;
        _incomeService = incomeService;
        _expenseService = expenseService;
    }

    public async Task RecordBankTransaction(BankTransactionDto dto)
    {
        var banktransaction = new BankTransactionDto
        {
            BankId = dto.BankId,
            TxnDate = dto.TxnDate,
            Amount = dto.Amount,
            Type = dto.Type,
            Remarks = dto.Remarks,
        };
        var banktxn = await _bankService.RecordBankTransactionAsync(banktransaction);
        var acctxn = _voucherService.RecordTransactionAsync(new AccTransactionDto
        {
            TxnDate = dto.TxnDate,
            Amount = dto.Amount,
            Type = dto.Type,
            TypeId = banktxn.Id,
            Remarks = dto.Remarks,
            IsJv = false,
            Details = new List<TransactionDetailDto>
            {
                new() { LedgerID = dto.BankId, IsDr = dto.Type == "Deposit", Amount = dto.Amount },
                new() { LedgerID = -3, IsDr = dto.Type != "Deposit", Amount = dto.Amount },
            }
        });
        await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(banktxn.Id, acctxn.Id);

        await _bankService.UpdateRemainingBalanceInBankAsync(dto.BankId);
    }
}