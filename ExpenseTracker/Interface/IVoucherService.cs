using ExpenseTracker.Dtos;
using Transaction = ExpenseTracker.Models.Transaction;

namespace TestApplication.ViewModels.Interface;

public interface IVoucherService
{
    Task<Transaction> RecordTransactionAsync(AccTransactionDto dto);
    Task<List<AccountingTransactionReportDto>> AccountingTransactionReportAsync();
    Task ReverseTransactionAsync(int transactionId);
    Task<List<VoucherDetailDto>> VoucherDetailAsync(int transactionId);
}

