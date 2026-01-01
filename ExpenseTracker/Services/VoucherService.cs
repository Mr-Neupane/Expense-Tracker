using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using Microsoft.EntityFrameworkCore;
using TestApplication.ViewModels.Interface;
using static ExpenseTracker.Providers.VoucherNumberProvider;
using Transaction = ExpenseTracker.Models.Transaction;

namespace ExpenseTracker.Services;

public class VoucherService : IVoucherService
{
    private readonly ApplicationDbContext _dbContext;

    public VoucherService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetNextJvVoucherNoAsync()
    {
        var numbers = await _dbContext.AccountingTransaction
            .AsNoTracking()
            .Where(x => x.VoucherNo.StartsWith("JV"))
            .Select(x => x.VoucherNo.Substring(2))
            .ToListAsync();

        var max = numbers
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();

        return $"JV{(max + 1):D4}";
    }


    public async Task<Transaction> RecordTransactionAsync(AccTransactionDto dto)
    {
        string voucherNo = dto.IsJv ? await GetNextJvVoucherNoAsync() : await GetVoucherNumber();
        var txn = new Transaction()
        {
            TxnDate = dto.TxnDate.ToUniversalTime(),
            VoucherNo = voucherNo,
            Amount = dto.Amount,
            Type = dto.Type,
            TypeId = dto.TypeId,
            Remarks = dto.Remarks,
            RecStatus = 'A',
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = 1,
            RecById = -1,
            TransactionDetails = dto.Details.Select(d => new TransactionDetail
            {
                LedgerId = d.LedgerID,
                DrAmount = d.IsDr ? d.Amount : 0,
                CrAmount = !d.IsDr ? d.Amount : 0,
                DrCr = d.IsDr ? 'D' : 'C',
                RecStatus = 'A',
                Status = 1,
                RecById = -1
            }).ToList()
        };
        await _dbContext.AccountingTransaction.AddAsync(txn);
        await _dbContext.SaveChangesAsync();
        return txn;
    }

    public async Task<List<AccountingTransactionReportDto>> AccountingTransactionReportAsync()
    {
        var accTransactionRepo = await (from t in _dbContext.AccountingTransaction
                join u in _dbContext.Users on t.RecById equals u.Id
                where t.Status == 1
                select new AccountingTransactionReportDto
                {
                    TransactionId = t.Id,
                    TxnDate = t.TxnDate,
                    VoucherNo = t.VoucherNo,
                    Remarks = t.Remarks,
                    Type = t.Type,
                    Username = u.Username,
                    Amount = t.Amount,
                    Status = t.Status
                }
            ).ToListAsync();
        return accTransactionRepo;
    }

    public async Task ReverseTransactionAsync(int transactionId)
    {
        var txn = await _dbContext.AccountingTransaction.Where(x => x.Id == transactionId && x.Status == 1)
            .Include(transaction => transaction.TransactionDetails)
            .ToListAsync();


        foreach (var t in txn)
        {
            t.Status = 2;
            foreach (var i in t.TransactionDetails.Select(td => td.Status = 2)) ;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<VoucherDetailDto>> VoucherDetailAsync(int transactionId)
    {
        var report = await (from t in _dbContext.AccountingTransaction
            join td in _dbContext.TransactionDetails on t.Id equals td.TransactionId
            join l in _dbContext.Ledgers on td.LedgerId equals l.Id
            join p in _dbContext.Ledgers on l.SubParentId equals p.Id
            join u in _dbContext.Users on t.RecById equals u.Id
            where t.Status == 1 && td.Status == 1 && td.TransactionId == transactionId
            select new VoucherDetailDto
            {
                LedgerName = string.Concat(p.Ledgername, " > ", l.Ledgername),
                Code = l.Code,
                VoucherNo = t.VoucherNo,
                DrAmount = td.DrAmount,
                CrAmount = td.CrAmount,
                Type = t.Type,
                Remarks = t.Remarks,
                TxnDate = t.TxnDate,
                UserName = u.Username
            }).ToListAsync();
        return report;
    }
}