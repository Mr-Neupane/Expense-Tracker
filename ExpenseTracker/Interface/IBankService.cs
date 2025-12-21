using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace TestApplication.ViewModels.Interface;

public interface IBankService
{
    Task<BankTransaction> RecordBankTransactionAsync(BankTransactionDto dto);
}