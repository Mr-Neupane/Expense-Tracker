using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace ExpenseTracker.Interface;

public interface IIncomeService
{
    Task<Income> RecordIncomeAsync(IncomeDto dto);
    Task ReverseIncomeAsync(int id);
    
    Task<List<IncomeReportDto> > GetIncomeReportAsync();
}