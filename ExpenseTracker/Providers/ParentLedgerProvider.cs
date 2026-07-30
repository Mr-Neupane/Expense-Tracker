using ExpenseTracker.Dtos;
using ExpenseTracker.Enums;
using ExpenseTracker.ExtMethods;
using ExpenseTracker.Providers.Interfaces;
using ExpenseTracker.Repository;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Providers;

public class ParentLedgerProvider : IParentLedgerProvider
{
    private readonly ILedgerRepo _ledgerRepo;
    private readonly ICoaLedgerRepo _coaLedgerRepo;

    public ParentLedgerProvider(ILedgerRepo ledgerRepo, ICoaLedgerRepo coaLedgerRepo)
    {
        _ledgerRepo = ledgerRepo;
        _coaLedgerRepo = coaLedgerRepo;
    }

    public async Task<List<ParentLedgerReportDto>> GetParentLedgerReport()
    {
        var ledgers = _ledgerRepo.GetBaseQueryable().FilterActiveStatus().Where(x => x.ParentId != null);
        var coa = _coaLedgerRepo.GetBaseQueryable().FilterActiveStatus();
        var res = await (from l in ledgers
                join c in coa on l.ParentId equals c.Id
                select new ParentLedgerReportDto
                {
                    ParentLedgerName = c.Name,
                    LedgerId = l.Id,
                    Status = l.Status,
                    LedgerCode = l.Code,
                    LedgerName = l.LedgerName,
                    UserName = l.RecBy.UserName,
                }
            ).ToListAsync();
        
        return res;

    }
}