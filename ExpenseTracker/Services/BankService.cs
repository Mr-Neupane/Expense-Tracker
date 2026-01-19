using Dapper;
using ExpenseTracker;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestApplication.Enums;
using TestApplication.ViewModels.Interface;

public class BankService : IBankService
{
    private readonly ApplicationDbContext _context;

    public BankService(ApplicationDbContext context)
    {
        _context = context;
    }


    

    public static async Task<List<dynamic>> GetBankTransactionReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var txnreport =
            await conn.QueryAsync(@"select b.id bankid,b.bankname,t.*,u.username
            from bank.banktransactions t
                join users u on u.id = t.rec_by_id
            join bank.bank b on b.id = bank_id where t.status=1");
        return txnreport.ToList();
    }


    public static async Task<int> GetBankIdByLedgerId(int ledgerid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select id from bank.bank where ledgerid=@ledgerid";
        int? bankid = await conn.QueryFirstOrDefaultAsync<int?>(query, new { ledgerid });
        return bankid ?? 0;
    }

    public async Task<List<Bank>> BankReportAsync()
    {
        var report = await _context.Banks.Where(b => b.Status == Status.Active.ToInt()).ToListAsync();
        return report;
    }

    public async Task EditBankAsync(BankDto dto)
    {
        var bank = await _context.Banks.FindAsync(dto.Id);
        if (bank.BankName != dto.BankName)
        {
            bank.BankName = dto.BankName;
        }

        if (bank.AccountNumber != dto.AccountNumber)
        {
            bank.AccountNumber = dto.AccountNumber;
        }

        if (bank.BankContactNumber != dto.BankContact)
        {
            bank.BankContactNumber = dto.BankContact;
        }

        if (bank.BankAddress != dto.BankAddress)
        {
            bank.BankAddress = dto.BankAddress;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<BankTransaction> RecordBankTransactionAsync(BankTransactionDto dto)
    {
        var banktransaction = new BankTransaction
        {
            BankId = dto.BankId,
            TxnDate = dto.TxnDate.ToUniversalTime(),
            Amount = dto.Amount,
            Type = dto.Type,
            Remarks = dto.Remarks,
            RecDate = DateTime.Now.ToUniversalTime(),
            RecById = -1,
            RecStatus = 'A',
            Status = Status.Active.ToInt(),
            TransactionId = 0
        };
        await _context.BankTransaction.AddAsync(banktransaction);
        await _context.SaveChangesAsync();
        return banktransaction;
    }

    public async Task UpdateAccountingTransactionIdInBankTransactionAsync(int id, int transactionId)
    {
        var txn = await _context.BankTransaction.Where(t => t.Id == id).ToListAsync();
        foreach (var t in txn)
        {
            t.TransactionId = transactionId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Bank> AddBankAsync(BankDto dto)
    {
        var bank = new Bank
        {
            BankName = dto.BankName,
            AccountNumber = dto.AccountNumber,
            BankContactNumber = dto.BankContact,
            LedgerId = dto.LedgerId,
            RemainingBalance = dto.RemainingBalance,
            BankAddress = dto.BankAddress,
            AccountOpendate = dto.AccountOpenDate,
            RecStatus = 'A',
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active.ToInt(),
            RecbyId = -1
        };

        await _context.Banks.AddAsync(bank);
        await _context.SaveChangesAsync();
        return bank;
    }

    public async Task UpdateRemainingBalanceInBankAsync(int bid)
    {
        var deposit = _context.BankTransaction.Where(t => t.Status == Status.Active.ToInt() && t.Type == "Deposit" && t.BankId == bid)
            .Sum(t => t.Amount);
        var withdraw = _context.BankTransaction.Where(t => t.Status == Status.Active.ToInt() && t.Type == "Withdraw" && t.BankId == bid)
            .Sum(t => t.Amount);
        var rembal = deposit - withdraw;
        var banks = await _context.Banks.Where(b => b.Id == bid).ToListAsync();
        foreach (var b in banks)
        {
            b.RemainingBalance = rembal;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ReverseBankTransactionAsync(int id, int transactionId)
    {
        var txn = await _context.BankTransaction.Where(t => t.Id == id || t.TransactionId == transactionId)
            .ToListAsync();
        foreach (var t in txn)
        {
            t.Status = Status.Reversed.ToInt();
        }

        await _context.SaveChangesAsync();
    }
}