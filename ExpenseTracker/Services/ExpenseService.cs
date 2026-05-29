using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUow _uow;
    private readonly IExpenseGenericRepository _expenseGenericRepo;
    private readonly ITransactionGenericRepository _txnRepo;
    private readonly IUserGenericRepository _userGenericRepo;

    public ExpenseService(IUow uow, IExpenseGenericRepository expenseGenericRepo,
        ITransactionGenericRepository txnRepo, IUserGenericRepository userGenericRepo)
    {
        _uow = uow;
        _expenseGenericRepo = expenseGenericRepo;
        _txnRepo = txnRepo;
        _userGenericRepo = userGenericRepo;
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
        await _uow.AddAsync(expense);
        await _uow.SaveChangesAsync();

        return expense;
    }

    public async Task<List<ExpenseReportDto>> GetExpenseReportsAsync()
    {
        var tQuery = _txnRepo.GetBaseQueryable();
        var eQuery = _expenseGenericRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var report = await (from t in tQuery
                join e in eQuery on t.TypeId equals e.Id
                join u in uQuery on e.RecById equals u.Id
                where t.Status == 1 && t.Type == "Expense" && e.Status == 1
                select new ExpenseReportDto
                {
                    LedgerId = e.LedgerId,
                    TransactionId = t.Id,
                    Amount = e.DrAmount,
                    TxnDate = t.TxnDate,
                    VoucherNo = t.VoucherNo,
                    Username = u.Username,
                    Status = e.Status,
                }).ToListAsync();
        return report;
    }

    public async Task ReverseRecordedExpenseAsync(int id)
    {
        var expense = await _expenseGenericRepo.FindOrThrowAsync(id);
        if (expense.Status == 1)
        {
            expense.Status = 2;
            await _uow.SaveChangesAsync();
        }
    }
}
