using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

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
            Status = StatusConstants.Active,
            RecById = UserConstants.AdminUser,
        };
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();

        return expense;
    }

    public async Task<List<ExpenseReportDto>> GetExpenseReportsAsync()
    {
        var report = await (from t in _context.AccountingTransaction
            join e in _context.Expenses on t.TypeId equals e.Id
            join u in _context.Users on e.RecById equals u.Id
            where t.Status == StatusConstants.Active && t.Type == "Expense" && e.Status == StatusConstants.Active
            select new ExpenseReportDto
            {
                LedgerId = 0,
                TransactionId = t.Id,
                Amount = e.DrAmount,
                TxnDate = t.TxnDate,
                VoucherNo = t.VoucherNo,
                Username = u.UserName,
                Status = e.Status,
            }).ToListAsync();
        return report;
    }

    public async Task ReverseRecordedExpenseAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense is { Status: StatusConstants.Active })
        {
            expense.Status = StatusConstants.Reversed;
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Expense not found");
        }
    }
}