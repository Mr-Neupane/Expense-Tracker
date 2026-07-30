using ExpenseTracker.Dtos;

namespace ExpenseTracker.Providers.Interfaces;

public interface IParentLedgerProvider
{
    Task<List<ParentLedgerReportDto>> GetParentLedgerReport();
}