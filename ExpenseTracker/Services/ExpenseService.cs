using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUow _uow;
    private readonly IExpenseRepo _expenseGenericRepo;
    private readonly IAccountingTransactionRepo _txnRepo;
    private readonly IUserRepo _userGenericRepo;

    public ExpenseService(IUow uow, IExpenseRepo expenseGenericRepo,
        IAccountingTransactionRepo txnRepo, IUserRepo userGenericRepo)
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
            RecStatus = RecordStatusConstants.Active,
            Status = Status.Active,
            RecById = UserConstants.AdminUser,
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
                where t.Status == Status.Active && t.Type == TransactionTypeConstants.Expense && e.Status == Status.Active
                select new ExpenseReportDto
                {
                    LedgerId = e.LedgerId,
                    TransactionId = t.Id,
                    Amount = e.DrAmount,
                    TxnDate = t.TxnDate,
                    VoucherNo = t.VoucherNo,
                    Username = u.UserName,
                    Status = (int)e.Status,
                }).ToListAsync();
        return report;
    }

    public async Task ReverseRecordedExpenseAsync(int id)
    {
        var expense = await _expenseGenericRepo.FindOrThrowAsync(id);
        if (expense.Status == Status.Active)
        {
            expense.Status = Status.Reversed;
            await _uow.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Expense not found");
        }
    }
}
