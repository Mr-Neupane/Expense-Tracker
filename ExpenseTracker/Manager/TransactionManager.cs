using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using TestApplication.Interface;
using TestApplication.ViewModels.Interface;

namespace TestApplication.Manager;

public class AccTransactionManager
{
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;
    private readonly IIncomeService _incomeService;
    private readonly IExpenseService _expenseService;
    private readonly ILiabilityService _liabilityService;

    public AccTransactionManager(IVoucherService voucherService, IBankService bankService, IIncomeService incomeService,
        IExpenseService expenseService, ILiabilityService liabilityService)
    {
        _voucherService = voucherService;
        _bankService = bankService;
        _incomeService = incomeService;
        _expenseService = expenseService;
        _liabilityService = liabilityService;
    }

    private async Task<Transaction> RecordVoucher(AccTransactionDto accTransaction, int typeId)
    {
        var acctxn = await _voucherService.RecordTransactionAsync(new AccTransactionDto
        {
            TxnDate = accTransaction.TxnDate,
            Amount = accTransaction.Amount,
            Type = accTransaction.Type,
            TypeId = typeId,
            Remarks = accTransaction.Remarks,
            IsJv = false,
            Details = accTransaction.Details,
        });
        return acctxn;
    }

    public async Task RecordBankTransaction(BankTransactionDto dto, AccTransactionDto accTransaction)
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
        var acctxn = await RecordVoucher(accTransaction, banktxn.Id);
        await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(banktxn.Id, acctxn.Id);
        await _bankService.UpdateRemainingBalanceInBankAsync(dto.BankId);
    }


    public async Task RecordExpenseTransaction(NewExpenseDto dto, AccTransactionDto txndto)
    {
        var expense = await _expenseService.RecordExpenseAsync(dto);

        var accTrans = await RecordVoucher(txndto, expense.Id);
        var bankid = await BankService.GetBankIdByLedgerId(dto.FromLedgerId);
        if (bankid != 0)
        {
            var bankTransaction = await _bankService.RecordBankTransactionAsync(new BankTransactionDto
            {
                BankId = bankid,
                TxnDate = dto.TxnDate,
                Amount = dto.Amount,
                Type = "Withdraw",
                Remarks = txndto.Remarks,
            });
            await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTransaction.Id,
                accTrans.Id);
            await _bankService.UpdateRemainingBalanceInBankAsync(bankTransaction.BankId);
        }
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

    public async Task RecordLiabilityTransaction(LiabilityDto lidto, AccTransactionDto txndto)
    {
        var liability = await _liabilityService.RecordLiabilityAsync(lidto);
        var accTransaction = await _voucherService.RecordTransactionAsync(new AccTransactionDto
        {
            TxnDate = txndto.TxnDate,
            Amount = txndto.Amount,
            Type = txndto.Type,
            TypeId = liability.Id,
            Remarks = txndto.Remarks,
            IsJv = false,
            Details = txndto.Details,
        });

        if (lidto.BankId != 0)
        {
            var banktransaction = await _bankService.RecordBankTransactionAsync(new BankTransactionDto
            {
                BankId = lidto.BankId,
                LedgerId = 0,
                TxnDate = lidto.TxnDate.ToLocalTime(),
                Amount = lidto.Amount,
                Type = "Deposit",
                Remarks = lidto.Remarks,
            });
            await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(banktransaction.Id,
                accTransaction.Id);
        }
    }
}