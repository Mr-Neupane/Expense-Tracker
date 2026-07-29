using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Enums;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Repository;

public class BankTransactionRepo : GenericRepository<BankTransaction>, IBankTransactionRepo
{
    private readonly IUserRepo _userRepo;
    private readonly IBankRepo _bankRepo;

    public BankTransactionRepo(ApplicationDbContext context, IUserRepo userRepo, IBankRepo bankRepo) : base(context)
    {
        _userRepo = userRepo;
        _bankRepo = bankRepo;
    }
    
    public async Task<List<BankTransactionReportDto>> BankTransactionReportAsync()
    {
        var btQuery = GetBaseQueryable();
        var uQuery = _userRepo.GetBaseQueryable();
        var bQuery = _bankRepo.GetBaseQueryable();

        var res = await (from bt in btQuery
                join u in uQuery on bt.RecById equals u.Id
                join b in bQuery on bt.BankId equals b.Id
                where bt.Status == Status.Active
                select new BankTransactionReportDto
                {
                    BankTransactionId = bt.Id,
                    Id = bt.Id,
                    TransactionId = bt.TransactionId,
                    BankId = bt.BankId,
                    BankName = b.BankName,
                    Type = bt.Type,
                    Amount = bt.Amount,
                    TxnDate = bt.TxnDate,
                    Username = u.UserName,
                }
            ).ToListAsync();
        return res;
    }
}
