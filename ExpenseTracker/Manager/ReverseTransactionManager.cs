using System.Transactions;
using TestApplication.Interface;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Manager;

public class ReverseTransactionManager
{
    private readonly IVoucherService _voucherService;
    private readonly IBankService _bankService;
    private readonly IIncomeService _incomeService;
    private readonly IExpenseService _expenseService;
    private readonly ILiabilityService _liabilityService;

    public ReverseTransactionManager(IVoucherService voucherService, IBankService bankService,
        IIncomeService incomeService, IExpenseService expenseService, ILiabilityService liabilityService)
    {
        _voucherService = voucherService;
        _bankService = bankService;
        _incomeService = incomeService;
        _expenseService = expenseService;
        _liabilityService = liabilityService;
    }

    private async Task ReverseAccountingTransaction(int transactionId)
    {
        await _voucherService.ReverseTransactionAsync(transactionId);
    }

    public async Task ReverseBankTransaction(int id, int transactionId, int bankId)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await ReverseAccountingTransaction(transactionId);
            await _bankService.ReverseBankTransactionAsync(id, transactionId);
            await _bankService.UpdateRemainingBalanceInBankAsync(bankId);
            scope.Complete();
        }
    }

    public async Task ReverseIncomeTransaction(int id, int transactionId)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await ReverseAccountingTransaction(transactionId);
            await _incomeService.ReverseIncomeAsync(id);
            scope.Complete();
        }
    }

    public async Task ReverseLiabilityTransaction(int id, int transactionId)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await ReverseAccountingTransaction(transactionId);
            await _liabilityService.ReverseLiabilityTransactionAsync(id);
            scope.Complete();
        }
    }

    public async Task ReverseExpenseTransaction(int id, int transactionId)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await ReverseAccountingTransaction(transactionId);
            await _expenseService.ReverseRecordedExpenseAsync(id);
            scope.Complete();
        }
    }

    public async Task ReverseJournalTransaction(int transactionId)
    {
        await ReverseAccountingTransaction(transactionId);
    }
}