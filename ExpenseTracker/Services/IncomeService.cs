using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Interface;

namespace ExpenseTracker.Services;

public class IncomeService : IIncomeService
{
    private readonly ApplicationDbContext _context;

    public IncomeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Income> RecordIncomeAsync(IncomeDto dto)
    {
        var income = new Income
        {
            LedgerId = dto.Ledgerid,
            DrAmount = dto.Amount,
            CrAmount = 0,
            TxnDate = dto.TxnDate,
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = 1,
            RecStatus = 'A',
            RecById = -1
        };
        await _context.Incomes.AddAsync(income);
        await _context.SaveChangesAsync();
        return income;
    }

    public async Task ReverseIncomeAsync(int id)
    {
        var income = await _context.Incomes.Where(i => i.Id == id && i.Status == 1).ToListAsync();

        foreach (var txn in income)
        {
            txn.Status = 2;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<IncomeReportDto>> GetIncomeReportAsync()
    {
        var report = await (from i in _context.Incomes
            join t in _context.AccountingTransaction on i.Id equals t.TypeId
            join u in _context.Users on i.RecById equals u.Id
            where t.Type == "Income" && i.Status == 1 && t.Status == 1
            select new IncomeReportDto
            {
                Id = i.Id,
                Amount = i.CrAmount,
                Date = i.TxnDate,
                VoucherNo = t.VoucherNo,
                TransactionId = t.Id,
                Username = u.Username,
                Status = i.Status,
            }).ToListAsync();
        return report;
    }
}