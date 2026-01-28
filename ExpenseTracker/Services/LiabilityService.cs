using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
using TestApplication.Interface;

namespace ExpenseTracker.Services;

public class LiabilityService : ILiabilityService
{
    private readonly ApplicationDbContext _context;

    public LiabilityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Liability> RecordLiabilityAsync(LiabilityDto dto)
    {
        var liability = new Liability
        {
            LedgerId = dto.LedgerId,
            DrAmount = 0,
            CrAmount = dto.Amount,
            TxnDate = dto.TxnDate.ToUniversalTime(),
            RecDate = DateTime.Now.ToUniversalTime(),
            RecStatus = 'A',
            Status = Status.Active.ToInt(),
            RecById = -1
        };
        await _context.Liabilities.AddRangeAsync(liability);
        await _context.SaveChangesAsync();
        return liability;
    }

    public async Task<List<LiabilityReportDto>> GetAllLiabilityReportAsync()
    {
        var report = await (from t in _context.AccountingTransaction
            // join td in _context.TransactionDetails on t.Id equals td.TransactionId
            join e in _context.Liabilities on t.TypeId equals e.Id
            join u in _context.Users on e.RecById equals u.Id
            where e.Status == Status.Active.ToInt() && t.Status == Status.Active.ToInt() && t.Type == "Liability"
            select new LiabilityReportDto
            {
                Id = e.Id,
                Ledgerid = 0,
                TxnDate = e.TxnDate,
                Transactionid = t.Id,
                Voucherno = t.VoucherNo,
                Status = t.Status,
                Username = u.Username,
                Amount = t.Amount,
                Remarks = t.Remarks
            }).ToListAsync();
        return report;
    }

    public async Task ReverseLiabilityTransactionAsync(int id)
    {
        var liability = await _context.Liabilities.FindAsync(id);
        if (liability is { Status: (int)Status.Active })
        {
            liability.Status = Status.Reversed.ToInt();
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Liability not found");
        }
    }
}