using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public interface IBankTransactionRepo : IGenericRepository<BankTransaction>
{
    Task<List<BankTransactionReportDto>> BankTransactionReportAsync();

}
