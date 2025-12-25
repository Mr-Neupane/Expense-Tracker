using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace TestApplication.ViewModels.Interface;

public interface IBankService
{
    Task<Bank> AddBankAsync(BankDto dto);
    Task<List<Bank>> BankReportAsync();
    Task EditBankAsync(BankDto dto );
    Task<BankTransaction> RecordBankTransactionAsync(BankTransactionDto dto);
    Task UpdateAccountingTransactionIdInBankTransactionAsync(int id, int transactionId);
    Task UpdateRemainingBalanceInBankAsync(int bid);
    Task ReverseBankTransactionAsync(int id, int transactionId);
}