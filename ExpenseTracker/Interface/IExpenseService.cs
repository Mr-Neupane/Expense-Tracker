using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace TestApplication.Interface;

public interface IExpenseService
{
    Task<Expense> RecordExpenseAsync(NewExpenseDto dto);
    Task<List<ExpenseReportDto>> GetExpenseReportsAsync();
    Task ReverseRecordedExpenseAsync(int id);
}

