using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
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
            Status = 1,
            RecById = -1
        };
        await _context.Liabilities.AddRangeAsync(liability);
        await _context.SaveChangesAsync();
        return liability;
    }
}