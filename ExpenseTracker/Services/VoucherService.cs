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

    public string GetNextJvVoucherNo()
    {
        var vouchernumber = _dbContext.AccountingTransaction
            .Where(x => x.VoucherNo.StartsWith("JV"))
            .Select(x => x.VoucherNo.Substring(2))
            .AsEnumerable()
            .Select(x => int.Parse(x))
            .DefaultIfEmpty(0)
            .Max();
        var nextVoucherNumber = "JV0000" + (vouchernumber + 1);
        return nextVoucherNumber;
    }


    public async Task<Transaction> RecordTransactionAsync(AccTransactionDto dto)
    {
        string voucherNo = dto.IsJv ? GetNextJvVoucherNo() : await GetVoucherNumber();
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
        var list = await (from l in _dbContext.Ledgers
            join td in _dbContext.TransactionDetails on l.Id equals td.LedgerId
            join t in _dbContext.AccountingTransaction on td.TransactionId equals t.Id
            join l2 in _dbContext.Ledgers on l.SubParentId equals l2.Id
            join u in _dbContext.Users on t.RecById equals u.Id
                into rep
            from user in rep.DefaultIfEmpty()
            where t.Status == 1 && td.Status == 1
            select new AccountingTransactionReportDto()
            {
                Ledgername = string.Concat(l2.Ledgername, '|', l.Ledgername),
                TxnDate = t.TxnDate,
                VoucherNo = t.VoucherNo,
                Amount = t.Amount,
                Status = t.Status,
                Id = t.Id,
                Type = t.Type,
                Lcode = l.Code,
                Username = user.Username,
                Remarks = t.Remarks,
            }).ToListAsync();
        return list;
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