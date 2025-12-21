using System.Runtime.InteropServices.ComTypes;
using System.Transactions;
using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
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
        using (var trans = DapperConnectionProvider.GetConnection().BeginTransaction())
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
            trans.Commit();
            return txn;
        }
    }
}