using TestApplication.Interface;
using TestApplication.ViewModels.Interface;

namespace TestApplication.Manager;

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
        await ReverseAccountingTransaction(transactionId);
        await _bankService.ReverseBankTransactionAsync(id, transactionId);
        await _bankService.UpdateRemainingBalanceInBankAsync(bankId);
    }

    public async Task ReverseIncomeTransaction(int id, int transactionId)
    {
        await ReverseAccountingTransaction(transactionId);
        await _incomeService.ReverseIncomeAsync(id);
    }

    public async Task ReverseLiabilityTransaction(int id, int transactionId)
    {
        await ReverseAccountingTransaction(transactionId);
        await _liabilityService.ReverseLiabilityTransactionAsync(id);
    }

    public async Task ReverseExpenseTransaction(int id, int transactionId)
    {
        await ReverseAccountingTransaction(transactionId);
        await _expenseService.ReverseRecordedExpenseAsync(id);
    }

    public async Task ReverseJournalTransaction(int transactionId)
    {
        await ReverseAccountingTransaction(transactionId);
    }
}