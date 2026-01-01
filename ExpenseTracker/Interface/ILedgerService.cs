using ExpenseTracker.Controllers;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using TestApplication.ViewModels;

namespace TestApplication.Interface;

public interface ILedgerService
{
    public Task<Ledger> AddLedgerAsync(LedgerDto dto);
    public Task<List<ParentLedgerReportDto>> GetParentLedgerReportAsync();
    public Task<List<LedgerReportDto>> GetLedgerReportAsync();
    public Task<List<LedgerStatement>> GetLedgerStatementsAsync(LedgerStatementDto dto);
}