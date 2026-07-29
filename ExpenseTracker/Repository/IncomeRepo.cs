using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Enums;
using ExpenseTracker.ExtMethods;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Repository;

public class IncomeRepo : GenericRepository<Income>, IIncomeRepo
{
    private readonly IAccountingTransactionRepo _txnRepo;
    private readonly IUserRepo _userRepo;

    public IncomeRepo(ApplicationDbContext context, IUserRepo userRepo, IAccountingTransactionRepo txnRepo) : base(context)
    {
        _userRepo = userRepo;
        _txnRepo = txnRepo;
    }
    
    public async Task<List<IncomeReportDto>> GetIncomeReportAsync()
    {
        var iQuery = GetBaseQueryable().FilterActiveStatus();
        var tQuery = _txnRepo.GetBaseQueryable();
        var uQuery = _userRepo.GetBaseQueryable();

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
