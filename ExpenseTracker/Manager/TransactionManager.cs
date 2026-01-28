using System.Transactions;
using ExpenseTracker.Dtos;
using ExpenseTracker.Providers;
using TestApplication.Interface;
using TestApplication.ViewModels.Interface;
using Transaction = ExpenseTracker.Models.Transaction;

namespace ExpenseTracker.Manager;

public class AccTransactionManager
{
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;
    private readonly IIncomeService _incomeService;
    private readonly IExpenseService _expenseService;
    private readonly ILiabilityService _liabilityService;
    private readonly IProvider _provider;

    public AccTransactionManager(IVoucherService voucherService, IBankService bankService, IIncomeService incomeService,
        IExpenseService expenseService, ILiabilityService liabilityService, IProvider provider)
    {
        _voucherService = voucherService;
        _bankService = bankService;
        _incomeService = incomeService;
        _expenseService = expenseService;
        _liabilityService = liabilityService;
        _provider = provider;
    }

    private async Task<Transaction> RecordVoucher(AccTransactionDto accTransaction, int typeId)
    {
        var accTxn = await _voucherService.RecordTransactionAsync(new AccTransactionDto
        {
            TxnDate = accTransaction.TxnDate,
            Amount = accTransaction.Amount,
            Type = accTransaction.Type,
            TypeId = typeId,
            Remarks = accTransaction.Remarks,
            IsJv = false,
            Details = accTransaction.Details,
        });
        return accTxn;
    }

    public async Task RecordBankTransaction(BankTransactionDto dto, AccTransactionDto accTransaction)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var bankTransaction = new BankTransactionDto
            {
                BankId = dto.BankId,
                TxnDate = dto.TxnDate,
                Amount = dto.Amount,
                Type = dto.Type,
                Remarks = dto.Remarks,
            };
            var bankTxn = await _bankService.RecordBankTransactionAsync(bankTransaction);
            var accTxn = await RecordVoucher(accTransaction, bankTxn.Id);
            await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTxn.Id, accTxn.Id);
            await _bankService.UpdateRemainingBalanceInBankAsync(dto.BankId);
            scope.Complete();
        }
    }


    public async Task RecordExpenseTransaction(NewExpenseDto dto, AccTransactionDto txnDto)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var expense = await _expenseService.RecordExpenseAsync(dto);

            var accTrans = await RecordVoucher(txnDto, expense.Id);
            var bankId = await _provider.GetBankIdByLedgerId(dto.FromLedgerId);
            if (bankId != 0)
            {
                var bankTransaction = await _bankService.RecordBankTransactionAsync(new BankTransactionDto
                {
                    BankId = bankId ?? 0,
                    TxnDate = dto.TxnDate,
                    Amount = dto.Amount,
                    Type = "Withdraw",
                    Remarks = txnDto.Remarks,
                });
                await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTransaction.Id,
                    accTrans.Id);
                await _bankService.UpdateRemainingBalanceInBankAsync(bankTransaction.BankId);
                
            }
            scope.Complete();
        }
    }

    public async Task RecordIncomeTransaction(IncomeDto idto, AccTransactionDto dto)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var income = await _incomeService.RecordIncomeAsync(idto);
            var transaction = await RecordVoucher(dto, income.Id);


            var bankId = await _provider.GetBankIdByLedgerId(idto.FromLedgerid);
            if (bankId != 0)
            {
                var bankTransaction = new BankTransactionDto
                {
                    BankId = bankId ?? 0,
                    TxnDate = dto.TxnDate.ToLocalTime(),
                    Amount = dto.Amount,
                    Type = "Deposit",
                    Remarks = dto.Remarks
                };
                await _bankService.RecordBankTransactionAsync(bankTransaction);
                await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTransaction.Id,
                    transaction.Id);
               
            }
            scope.Complete();
        }
    }

    public async Task RecordLiabilityTransaction(LiabilityDto liDto, AccTransactionDto txnDto)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var liability = await _liabilityService.RecordLiabilityAsync(liDto);
            var accTransaction = await RecordVoucher(txnDto, liability.Id);

            if (liDto.BankId != 0)
            {
                var bankTransaction = await _bankService.RecordBankTransactionAsync(new BankTransactionDto
                {
                    BankId = liDto.BankId,
                    LedgerId = 0,
                    TxnDate = liDto.TxnDate.ToLocalTime(),
                    Amount = liDto.Amount,
                    Type = "Deposit",
                    Remarks = liDto.Remarks,
                });
                await _bankService.UpdateAccountingTransactionIdInBankTransactionAsync(bankTransaction.Id,
                    accTransaction.Id);
            }

            scope.Complete();
        }
    }
}