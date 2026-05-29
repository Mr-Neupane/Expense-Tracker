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

public class IncomeService : IIncomeService
{
    private readonly IUow _uow;
    private readonly IIncomeGenericRepository _incomeGenericRepo;
    private readonly ITransactionGenericRepository _txnRepo;
    private readonly IUserGenericRepository _userGenericRepo;

    public IncomeService(IUow uow, IIncomeGenericRepository incomeGenericRepo,
        ITransactionGenericRepository txnRepo, IUserGenericRepository userGenericRepo)
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
            Status = Status.Active.ToInt(),
            RecStatus = 'A',
            RecById = UserConstants.AdminUser
        };
        await _uow.AddAsync(income);
        await _uow.SaveChangesAsync();
        return income;
    }

    public async Task ReverseIncomeAsync(int id)
    {
        var income = await _incomeGenericRepo.FindOrThrowAsync(id);
        if (income.Status == Status.Active.ToInt())
        {
            income.Status = Status.Reversed.ToInt();
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<List<IncomeReportDto>> GetIncomeReportAsync()
    {
<<<<<<< HEAD
        var report = await (from i in _context.Incomes
            join t in _context.AccountingTransaction on i.Id equals t.TypeId
            join u in _context.Users on i.RecById equals u.Id
            where t.Type == "Income" && i.Status == Status.Active.ToInt() && t.Status == Status.Active.ToInt()
            select new IncomeReportDto
            {
                Id = i.Id,
                Amount = i.CrAmount,
                Date = i.TxnDate,
                VoucherNo = t.VoucherNo,
                TransactionId = t.Id,
                Username = u.UserName,
                Status = i.Status,
            }).ToListAsync();
=======
        var iQuery = _incomeGenericRepo.GetBaseQueryable();
        var tQuery = _txnRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var report = await (from i in iQuery
                join t in tQuery on i.Id equals t.TypeId
                join u in uQuery on i.RecById equals u.Id
                where t.Type == "Income" && i.Status == Status.Active.ToInt() && t.Status == Status.Active.ToInt()
                select new IncomeReportDto
                {
                    Id = i.Id,
                    Amount = i.CrAmount,
                    Date = i.TxnDate,
                    VoucherNo = t.VoucherNo,
                    TransactionId = t.Id,
                    Username = u.Username,
                    Status = i.Status,
                }).ToListAsync();
>>>>>>> main
        return report;
    }
}
