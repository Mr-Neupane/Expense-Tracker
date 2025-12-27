using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Interface;

namespace ExpenseTracker.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Expense> RecordExpenseAsync(NewExpenseDto dto)
    {
        var expense = new Expense
        {
            LedgerId = dto.LedgerId,
            DrAmount = dto.Amount,
            CrAmount = 0,
            TxnDate = dto.TxnDate.ToUniversalTime(),
            RecDate = DateTime.Now.ToUniversalTime(),
            RecStatus = 'A',
            Status = 1,
            RecById = -1,
        };
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();

        return expense;
    }

    public async Task<List<ExpenseReportDto>> GetExpenseReportsAsync()
    {
        var report = await (from t in _context.AccountingTransaction
            // join td in _context.TransactionDetails on t.Id equals td.TransactionId
            join e in _context.Expenses on t.TypeId equals e.Id
            join u in _context.Users on e.RecById equals u.Id
            where t.Status == 1 && t.Type == "Expense" && e.Status == 1
            select new ExpenseReportDto
            {
                LedgerId = 0,
                TransactionId = t.Id,
                Amount = e.DrAmount,
                TxnDate = t.TxnDate,
                VoucherNo = t.VoucherNo,
                Username = u.Username,
                Status = e.Status,
            }).ToListAsync();
        return report;
    }
}