using System.Transactions;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
using TestApplication.ViewModels.Interface;
using Transaction = ExpenseTracker.Models.Transaction;

namespace ExpenseTracker.Services;

public class VoucherService : IVoucherService
{
    private readonly ApplicationDbContext _dbContext;

    public VoucherService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Transaction> RecordTransactionAsync(AccTransactionDto dto)
    {
        var voucherNo = GetVoucherNo(dto.IsJv);
        var txn = new Transaction
        {
            TxnDate = dto.TxnDate.ToUniversalTime(),
            VoucherType = dto.IsJv ? (int)VoucherType.Journal : (int)VoucherType.Automatic,
            VoucherNo = voucherNo,
            Amount = dto.Amount,
            Type = dto.Type,
            TypeId = dto.TypeId,
            Remarks = dto.Remarks,
            RecStatus = 'A',
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active.ToInt(),
            RecById = -1,
            TransactionDetails = dto.Details.Select(d => new TransactionDetail
            {
                LedgerId = d.LedgerID,
                DrAmount = d.IsDr ? d.Amount : 0,
                CrAmount = !d.IsDr ? d.Amount : 0,
                DrCr = d.IsDr ? 'D' : 'C',
                RecStatus = 'A',
                Status = Status.Active.ToInt(),
                RecById = -1
            }).ToList()
        };
        await _dbContext.AccountingTransaction.AddAsync(txn);
        await _dbContext.SaveChangesAsync();
        return txn;
    }

    public async Task<List<AccountingTransactionReportDto>> AccountingTransactionReportAsync(TransactionReportDto dto)
    {
        var accTransactionRepo = await (from t in _dbContext.AccountingTransaction
                join u in _dbContext.Users on t.RecById equals u.Id
                where (dto.Status == 0 || dto.Status == t.Status) && t.TxnDate.Date >= dto.DateFrom.Date &&
                      t.TxnDate.Date <= dto.DateTo.Date && (dto.Type == "All" || dto.Type == t.Type)
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
        var transaction = await _dbContext.AccountingTransaction.SingleOrDefaultAsync(x => x.Id == transactionId);
        if (transaction != null)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var txn = await _dbContext.TransactionDetails.Where(t => t.TransactionId == transactionId)
                    .ToListAsync();
                var revTxn = await RecordReverseTransactionAsync(transactionId);
                {
                    transaction.Status = Status.Reversed.ToInt();
                    transaction.IsReversed = true;
                    transaction.ReversedId = revTxn.Id;
                    txn.ForEach(a => a.Status = Status.Reversed.ToInt());
                    await _dbContext.SaveChangesAsync();
                }


                scope.Complete();
            }
        }
        else
        {
            throw new Exception("Transaction not found");
        }
    }

    private async Task<Transaction> RecordReverseTransactionAsync(int transactionId)
    {
        var existingTransaction =
            await _dbContext.AccountingTransaction.Where(x => x.Id == transactionId).Select(x => x.Id)
                .SingleOrDefaultAsync();
        if (existingTransaction == null || transactionId == 0)
        {
            throw new Exception("Transaction not found");
        }
        else
        {
            var voucherNo = GetVoucherNo(null, transactionId, true);
            var txn = (from t in _dbContext.AccountingTransaction
                where t.Id == transactionId
                select new AccTransactionDto
                {
                    TxnDate = DateTime.Now.ToUniversalTime(),
                    Amount = t.Amount,
                    Type = t.Type,
                    TypeId = t.TypeId,
                    Remarks = "Reverse transaction",
                    IsJv = t.VoucherType == (int)VoucherType.Journal,
                }).Single();

            var txnDetail = _dbContext.TransactionDetails.Where(t => t.TransactionId == transactionId).ToList();
            var newTxn = new Transaction
            {
                TxnDate = txn.TxnDate,
                VoucherType = txn.IsJv ? (int)VoucherType.Automatic : (int)VoucherType.Journal,
                VoucherNo = voucherNo,
                Amount = txn.Amount,
                Type = txn.Type,
                TypeId = txn.TypeId,
                Remarks = txn.Remarks,
                IsReversed = true,
                ReversedId = null,
                TransactionDetails = txnDetail.Select(x => new TransactionDetail
                {
                    LedgerId = x.LedgerId,
                    DrAmount = x.CrAmount,
                    CrAmount = x.DrAmount,
                    DrCr = x.DrCr != 'D' ? 'D' : 'C',
                }).ToList()
            };
            await _dbContext.AccountingTransaction.AddRangeAsync(newTxn);
            await _dbContext.SaveChangesAsync();
            return newTxn;
        }
    }

    public async Task<List<VoucherDetailDto>> VoucherDetailAsync(int transactionId)
    {
        var report = await (from t in _dbContext.AccountingTransaction
            join td in _dbContext.TransactionDetails on t.Id equals td.TransactionId
            join l in _dbContext.Ledgers on td.LedgerId equals l.Id
            join p in _dbContext.Ledgers on l.SubParentId equals p.Id
            join u in _dbContext.Users on t.RecById equals u.Id
            where td.TransactionId == transactionId
            select new VoucherDetailDto
            {
                LedgerName = string.Concat(p.LedgerName,
                    " > ",
                    l.LedgerName),
                Code = l.Code,
                VoucherNo = t.VoucherNo,
                DrAmount = td.DrAmount,
                CrAmount = td.CrAmount,
                Type = t.Type,
                TransactionId = td.TransactionId,
                Status = t.Status,
                Typeid = t.TypeId,
                Remarks = t.Remarks,
                TxnDate = t.TxnDate,
                UserName = u.Username,
                IsReverseVoucher = t.IsReversed
            }).ToListAsync();
        return report;
    }

    private string GetVoucherNo(bool? isJv = null, int? transactionId = null, bool? isReverse = null)
    {
        if (isReverse.HasValue.Equals(true))
        {
            var txn = _dbContext.AccountingTransaction.Where(x => x.Id == transactionId).Select(x => x.VoucherNo)
                .SingleOrDefault();
            const string revPref = "Rev-";

            if (txn == null)
            {
                throw new Exception("Transaction with supplied parm not found");
            }
            else
            {
                var voucherNo = string.Concat(revPref, txn);
                return voucherNo;
            }
        }

        if (isJv.HasValue)
        {
            if (isJv.Equals(true))
            {
                var cn = _dbContext.AccountingTransaction.Count(x => x.VoucherType == (int)VoucherType.Journal);
                var voucherNo = string.Concat("JV00000", (cn + 1));
                return voucherNo;
            }
            else
            {
                var cn = _dbContext.AccountingTransaction.Count(x => x.VoucherType == (int)VoucherType.Automatic);
                var voucherNo = string.Concat("AV00000", (cn + 1));
                return voucherNo;
            }
        }
        else
        {
            throw new Exception("Transaction with supplied parm not found");
        }
    }
}