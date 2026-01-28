using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
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
            DrAmount = 0,
            CrAmount = dto.Amount,
            TxnDate = dto.TxnDate,
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active.ToInt(),
            RecStatus = 'A',
            RecById = -1
        };
        await _context.Incomes.AddAsync(income);
        await _context.SaveChangesAsync();
        return income;
    }

    public async Task ReverseIncomeAsync(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income is { Status: (int)Status.Active })
        {
            income.Status = Status.Reversed.ToInt();
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Income not found");
        }
    }

    public async Task<List<IncomeReportDto>> GetIncomeReportAsync()
    {
        var report = await (from i in _context.Incomes
            join t in _context.AccountingTransaction on i.Id equals t.TypeId
            join u in _context.Users on i.RecById equals u.Id
            where t.Type == "Income" && i.Status == Status.Active.ToInt() && t.Status == Status.Active.ToInt()
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