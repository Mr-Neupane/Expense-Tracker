using System.Transactions;
using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;
using ExpenseTracker.ViewModels.Interface;
using Transaction = ExpenseTracker.Models.Transaction;

namespace ExpenseTracker.Services;

public class VoucherService : IVoucherService
{
    private readonly IUow _uow;
    private readonly IAccountingTransactionRepo _txnRepo;
    private readonly IAccTxnDetailRepo _txnDetailGenericRepo;
    private readonly ILedgerRepo _ledgerGenericRepo;
    private readonly IUserRepo _userGenericRepo;

    public VoucherService(IUow uow, IAccountingTransactionRepo txnRepo,
        IAccTxnDetailRepo txnDetailGenericRepo, ILedgerRepo ledgerGenericRepo,
        IUserRepo userGenericRepo)
    {
        _uow = uow;
        _txnRepo = txnRepo;
        _txnDetailGenericRepo = txnDetailGenericRepo;
        _ledgerGenericRepo = ledgerGenericRepo;
        _userGenericRepo = userGenericRepo;
    }

    public async Task<Transaction> RecordTransactionAsync(AccTransactionDto dto)
    {
        var voucherNo = GetVoucherNo(dto.IsJv);
        var txn = new Transaction
        {
            TxnDate = dto.TxnDate.ToUniversalTime(),
            VoucherType = dto.IsJv ? VoucherType.Journal : VoucherType.Automatic,
            VoucherNo = voucherNo,
            Amount = dto.Amount,
            Type = dto.Type,
            TypeId = dto.TypeId,
            Remarks = dto.Remarks,
            RecStatus = RecordStatusConstants.Active,
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active,
            RecById = UserConstants.AdminUser,
            TransactionDetails = dto.Details.Select(d => new TransactionDetail
            {
                LedgerId = d.LedgerID,
                DrAmount = d.IsDr ? d.Amount : 0,
                CrAmount = !d.IsDr ? d.Amount : 0,
                DrCr = d.IsDr ? DrCrConstants.Dr : DrCrConstants.Cr,
                RecStatus = RecordStatusConstants.Active,
                Status = Status.Active,
                RecById = UserConstants.AdminUser
            }).ToList()
        };
        await _uow.AddAsync(txn);
        await _uow.SaveChangesAsync();
        return txn;
    }

    public async Task<List<AccountingTransactionReportDto>> AccountingTransactionReportAsync(TransactionReportDto dto)
    {
        var tQuery = _txnRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var accTransactionRepo = await (from t in tQuery
                join u in uQuery on t.RecById equals u.Id
                where (dto.Status == 0 || dto.Status == (int)t.Status) && t.TxnDate.Date >= dto.DateFrom.Date &&
                      t.TxnDate.Date <= dto.DateTo.Date && (dto.Type == TransactionTypeConstants.All || dto.Type == t.Type)
                select new AccountingTransactionReportDto
                {
                    TransactionId = t.Id,
                    TxnDate = t.TxnDate,
                    VoucherNo = t.VoucherNo,
                    Remarks = t.Remarks,
                    Type = t.Type,
                    Username = u.UserName,
                    Amount = t.Amount,
                    Status = (int)t.Status
                }
            ).ToListAsync();
        return accTransactionRepo;
    }

    public async Task ReverseTransactionAsync(int transactionId)
    {
        var transaction = await _txnRepo.SingleOrDefaultAsync(x => x.Id == transactionId);
        if (transaction != null)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var txn = await _txnDetailGenericRepo.GetAsync(t => t.TransactionId == transactionId);
                var revTxn = await RecordReverseTransactionAsync(transactionId);
                {
                    transaction.Status = Status.Reversed;
                    transaction.IsReversed = true;
                    transaction.ReversedId = revTxn.Id;
                    txn.ForEach(a => a.Status = Status.Reversed);
                    await _uow.SaveChangesAsync();
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
        var tQuery = _txnRepo.GetBaseQueryable();

        var exists = await tQuery.AnyAsync(x => x.Id == transactionId);
        if (!exists || transactionId == 0)
        {
            throw new Exception("Transaction not found");
        }
        else
        {
            var voucherNo = GetVoucherNo(null, transactionId, true);
            var txn = await (from t in tQuery
                where t.Id == transactionId
                select new AccTransactionDto
                {
                    TxnDate = DateTime.Now.ToUniversalTime(),
                    Amount = t.Amount,
                    Type = t.Type,
                    TypeId = t.TypeId,
                    Remarks = VoucherNumberConstants.ReverseRemarks,
                    IsJv = t.VoucherType == VoucherType.Journal,
                }).SingleAsync();

            var txnDetail = await _txnDetailGenericRepo.GetAsync(t => t.TransactionId == transactionId);
            var newTxn = new Transaction
            {
                TxnDate = txn.TxnDate,
                VoucherType = txn.IsJv ? VoucherType.Automatic : VoucherType.Journal,
                VoucherNo = voucherNo,
                Amount = txn.Amount,
                Type = txn.Type,
                TypeId = txn.TypeId,
                Remarks = txn.Remarks,
                IsReversed = true,
                ReversedId = null,
                RecStatus = RecordStatusConstants.Active,
                Status = Status.Active,
                RecDate = DateTime.UtcNow,
                RecById = UserConstants.AdminUser,
                TransactionDetails = txnDetail.Select(x => new TransactionDetail
                {
                    LedgerId = x.LedgerId,
                    DrAmount = x.CrAmount,
                    CrAmount = x.DrAmount,
                    DrCr = x.DrCr != DrCrConstants.Dr ? DrCrConstants.Cr : DrCrConstants.Dr,
                    RecStatus = RecordStatusConstants.Active,
                    Status = Status.Active,
                    RecDate = DateTime.UtcNow,
                    RecById = UserConstants.AdminUser,
                }).ToList()
            };
            await _uow.AddAsync(newTxn);
            await _uow.SaveChangesAsync();
            return newTxn;
        }
    }

    public async Task<List<VoucherDetailDto>> VoucherDetailAsync(int transactionId)
    {
        var tQuery = _txnRepo.GetBaseQueryable();
        var tdQuery = _txnDetailGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var report = await (from t in tQuery
                join td in tdQuery on t.Id equals td.TransactionId
                join l in lQuery on td.LedgerId equals l.Id
                join p in lQuery on l.SubParentId equals p.Id
                join u in uQuery on t.RecById equals u.Id
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
                    Status = (int)t.Status,
                    Typeid = t.TypeId,
                    Remarks = t.Remarks,
                    TxnDate = t.TxnDate,
                    UserName = u.UserName,
                    IsReverseVoucher = t.IsReversed
                }).ToListAsync();
        return report;
    }

    private string GetVoucherNo(bool? isJv = null, int? transactionId = null, bool? isReverse = null)
    {
        if (isReverse == true)
        {
            var txn = _txnRepo.GetBaseQueryable()
                .Where(x => x.Id == transactionId).Select(x => x.VoucherNo)
                .SingleOrDefault();
            const string revPref = "Rev-";

            if (txn == null)
            {
                throw new KeyNotFoundException($"Transaction with id {transactionId} not found");
            }
            else
            {
                var voucherNo = string.Concat(revPref, txn);
                return voucherNo;
            }

        }

        if (isJv.HasValue)
        {
            if (isJv.Value)
            {
                var cn = _txnRepo.GetBaseQueryable()
                    .Count(x => x.VoucherType == VoucherType.Journal);
                var voucherNo = $"{VoucherNumberConstants.JvPrefix}{(cn + 1):D6}";
                return voucherNo;
            }
            else
            {
                var cn = _txnRepo.GetBaseQueryable()
                    .Count(x => x.VoucherType == VoucherType.Automatic);
                var voucherNo = $"{VoucherNumberConstants.AutoPrefix}{(cn + 1):D6}";
                return voucherNo;
            }
        }
        else
        {
            throw new InvalidOperationException("Voucher number cannot be generated: no voucher type was specified");
        }
    }
}
