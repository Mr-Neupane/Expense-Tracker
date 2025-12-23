using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace TestApplication.Interface;

public interface IIncomeService
{
    Task<Income> RecordIncomeAsync(IncomeDto dto);
    Task ReverseIncomeAsync(int id);
    
    Task<List<IncomeReportDto> > GetIncomeReportAsync();
}