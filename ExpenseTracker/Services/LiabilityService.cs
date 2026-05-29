<<<<<<< HEAD
using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
=======
﻿using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
>>>>>>> main
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;
using ExpenseTracker.Interface;

namespace ExpenseTracker.Services;

public class LiabilityService : ILiabilityService
{
    private readonly IUow _uow;
    private readonly ILiabilityGenericRepository _liabilityGenericRepo;
    private readonly ITransactionGenericRepository _txnRepo;
    private readonly IUserGenericRepository _userGenericRepo;

    public LiabilityService(IUow uow, ILiabilityGenericRepository liabilityGenericRepo,
        ITransactionGenericRepository txnRepo, IUserGenericRepository userGenericRepo)
    {
        _uow = uow;
        _liabilityGenericRepo = liabilityGenericRepo;
        _txnRepo = txnRepo;
        _userGenericRepo = userGenericRepo;
    }

    public async Task<Liability> RecordLiabilityAsync(LiabilityDto dto)
    {
        var liability = new Liability
        {
            LedgerId = dto.LedgerId,
            DrAmount = 0,
            CrAmount = dto.Amount,
            TxnDate = dto.TxnDate.ToUniversalTime(),
            RecDate = DateTime.Now.ToUniversalTime(),
            RecStatus = 'A',
            Status = Status.Active.ToInt(),
            RecById = UserConstants.AdminUser
        };
        await _uow.AddAsync(liability);
        await _uow.SaveChangesAsync();
        return liability;
    }

    public async Task<List<LiabilityReportDto>> GetAllLiabilityReportAsync()
    {
<<<<<<< HEAD
        var report = await (from t in _context.AccountingTransaction
            // join td in _context.TransactionDetails on t.Id equals td.TransactionId
            join e in _context.Liabilities on t.TypeId equals e.Id
            join u in _context.Users on e.RecById equals u.Id
            where e.Status == Status.Active.ToInt() && t.Status == Status.Active.ToInt() && t.Type == "Liability"
            select new LiabilityReportDto
            {
                Id = e.Id,
                Ledgerid = 0,
                TxnDate = e.TxnDate,
                Transactionid = t.Id,
                Voucherno = t.VoucherNo,
                Status = t.Status,
                Username = u.UserName,
                Amount = t.Amount,
                Remarks = t.Remarks
            }).ToListAsync();
=======
        var tQuery = _txnRepo.GetBaseQueryable();
        var eQuery = _liabilityGenericRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var report = await (from t in tQuery
                join e in eQuery on t.TypeId equals e.Id
                join u in uQuery on e.RecById equals u.Id
                where e.Status == Status.Active.ToInt() && t.Status == Status.Active.ToInt() && t.Type == "Liability"
                select new LiabilityReportDto
                {
                    Id = e.Id,
                    Ledgerid = e.LedgerId,
                    TxnDate = e.TxnDate,
                    Transactionid = t.Id,
                    Voucherno = t.VoucherNo,
                    Status = t.Status,
                    Username = u.Username,
                    Amount = t.Amount,
                    Remarks = t.Remarks
                }).ToListAsync();
>>>>>>> main
        return report;
    }

    public async Task ReverseLiabilityTransactionAsync(int id)
    {
        var liability = await _liabilityGenericRepo.FindOrThrowAsync(id);
        if (liability.Status == Status.Active.ToInt())
        {
            liability.Status = Status.Reversed.ToInt();
            await _uow.SaveChangesAsync();
        }
    }
}
