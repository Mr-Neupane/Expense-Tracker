using ExpenseTracker.Controllers;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using TestApplication.ViewModels;

namespace TestApplication.Interface;

public interface ILedgerService
{
    public Task<Ledger> AddLedgerAsync(LedgerDto dto);
    public Task EditLedgerAsync(EditLedgerDto dto);
    public Task<List<ParentLedgerReportDto>> GetParentLedgerReportAsync();
    public Task<List<LedgerReportDto>> GetLedgerReportAsync();
    public Task<bool> DeactivateLedgerAsync(int ledgerId);
    public Task ActivateLedgerAsync(int ledgerId);
    public Task<List<LedgerStatement>> GetLedgerStatementsAsync(LedgerStatementDto dto);
}

public class EditLedgerDto
{
    public int LedgerId { get; set; }
    public string LedgerName { get; set; }
}