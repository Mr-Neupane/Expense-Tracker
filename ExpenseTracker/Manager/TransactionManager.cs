using System.Transactions;
using ExpenseTracker.Dtos;
using TestApplication.Interface;
using TestApplication.ViewModels.Interface;

namespace TestApplication.Manager;

public class AccTransactionManager
{
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;
    private readonly IIncomeService _incomeService;
    private readonly IExpenseService _expenseService;

    public AccTransactionManager(IVoucherService voucherService, IBankService bankService, IIncomeService incomeService,
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
        var acctxn = await _voucherService.RecordTransactionAsync(new AccTransactionDto
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

    public async Task RecordIncomeTransaction(IncomeDto idto, AccTransactionDto dto)
    {
        var income = await _incomeService.RecordIncomeAsync(idto);
        var transaction = await _voucherService.RecordTransactionAsync(new AccTransactionDto
        {
            TxnDate = dto.TxnDate,
            Amount = dto.Amount,
            Type = dto.Type,
            TypeId = income.Id,
            Remarks = dto.Remarks,
            IsJv = false,
            Details = dto.Details,
        });

        int bankid = await BankService.GetBankIdByLedgerId(idto.FromLedgerid);
        if (bankid != 0)
        {
            var bankTransaction = new BankTransactionDto
            {
                BankId = bankid,
                TxnDate = dto.TxnDate.ToLocalTime(),
                Amount = dto.Amount,
                Type = "Deposit",
                Remarks = dto.Remarks
            };
            await _bankService.RecordBankTransactionAsync(bankTransaction);
            await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTransaction.Id,
                transaction.Id);
        }
    }
}