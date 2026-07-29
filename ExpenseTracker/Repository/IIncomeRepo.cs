using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public interface IIncomeRepo : IGenericRepository<Income>
{
    Task<List<IncomeReportDto>> GetIncomeReportAsync();
}
