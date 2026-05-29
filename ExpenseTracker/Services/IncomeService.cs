using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Services;

public class IncomeService : IIncomeService
{
    private readonly IUow _uow;
    private readonly IIncomeRepo _incomeGenericRepo;
    private readonly IAccountingTransactionRepo _txnRepo;
    private readonly IUserRepo _userGenericRepo;

    public IncomeService(IUow uow, IIncomeRepo incomeGenericRepo,
        IAccountingTransactionRepo txnRepo, IUserRepo userGenericRepo)
    {
        _uow = uow;
        _incomeGenericRepo = incomeGenericRepo;
        _txnRepo = txnRepo;
        _userGenericRepo = userGenericRepo;
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
            Status = Status.Active,
            RecStatus = RecordStatusConstants.Active,
            RecById = UserConstants.AdminUser
        };
        await _uow.AddAsync(income);
        await _uow.SaveChangesAsync();
        return income;
    }

    public async Task ReverseIncomeAsync(int id)
    {
        var income = await _incomeGenericRepo.FindOrThrowAsync(id);
        if (income.Status == Status.Active)
        {
            income.Status = Status.Reversed;
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<List<IncomeReportDto>> GetIncomeReportAsync()
    {
        var iQuery = _incomeGenericRepo.GetBaseQueryable();
        var tQuery = _txnRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var report = await (from i in iQuery
                join t in tQuery on i.Id equals t.TypeId
                join u in uQuery on i.RecById equals u.Id
                where t.Type == TransactionTypeConstants.Income && i.Status == Status.Active && t.Status == Status.Active
                select new IncomeReportDto
                {
                    Id = i.Id,
                    Amount = i.CrAmount,
                    Date = i.TxnDate,
                    VoucherNo = t.VoucherNo,
                    TransactionId = t.Id,
                    Username = u.UserName,
                    Status = (int)i.Status,
                }).ToListAsync();
        return report;
    }
}
