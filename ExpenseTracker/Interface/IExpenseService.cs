using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace ExpenseTracker.Interface;

public interface IExpenseService
{
    Task<Expense> RecordExpenseAsync(NewExpenseDto dto);
    Task<List<ExpenseReportDto>> GetExpenseReportsAsync();
    Task ReverseRecordedExpenseAsync(int id);
}

