using Dapper;
using ExpenseTracker;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestApplication.ViewModels.Interface;

public class BankService : IBankService
{
    private readonly ApplicationDbContext _context;

    public BankService(ApplicationDbContext context)
    {
        _context = context;
    }

    public static async Task<int> RecordBankTransaction(BankTransactionVm vm)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    var engdate = await DateHelper.GetEnglishDate(vm.TxnDate);
                    var banktran =
                        @"INSERT INTO bank.banktransactions ( bank_id,txn_date,amount,type,remarks,rec_date,rec_by_id,rec_status,status,transaction_id)
                                    values (@bank_id, @txn_date, @amount, @type, @remarks, @rec_date,@rec_by_id,@recs_tatus,@status,@transaction_id) returning id";
                    var id = await con.QuerySingleAsync<int>(banktran, new
                    {
                        bank_id = vm.BankId,
                        txn_date = engdate,
                        amount = vm.Amount,
                        type = vm.Type,
                        remarks = vm.Remarks,
                        rec_date = DateTime.Now,
                        rec_by_id = -1,
                        recs_tatus = vm.RecStatus,
                        status = vm.Status,
                        transaction_id = 0,
                    });
                    await txn.CommitAsync();
                    await con.CloseAsync();
                    return id;
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await con.CloseAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public static async Task UpdateTransactionDuringBankTransaction(int btid, int transactionid)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                var query = @"UPDATE bank.banktransactions SET transaction_id = @transactionid where id =@id";
                await con.ExecuteAsync(query, new { transactionid = transactionid, id = btid });
                await txn.CommitAsync();
                await con.CloseAsync();
            }
        }
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


    public static async Task<List<dynamic>> GetBankTransactionList(int transactionid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select bank_id, ledgerid, type, amount
from bank.banktransactions bt
         join bank.bank b on b.id = bt.bank_id
where transaction_id=@transactionid ;";

        var res = await conn.QueryAsync(query, new { transactionid });
        return res.ToList();
    }


    public async Task<List<Bank>> BankReportAsync()
    {
        var report = await _context.Banks.Where(b => b.Status == 1).ToListAsync();
        return report;
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
            Status = 1,
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
            Status = 1,
            RecbyId = -1
        };

        await _context.Banks.AddAsync(bank);
        await _context.SaveChangesAsync();
        return bank;
    }

    public async Task UpdateRemainingBalanceInBankAsync(int bid)
    {
        var deposit = _context.BankTransaction.Where(t => t.Status == 1 && t.Type == "Deposit" && t.BankId == bid)
            .Sum(t => t.Amount);
        var withdraw = _context.BankTransaction.Where(t => t.Status == 1 && t.Type == "Withdraw" && t.BankId == bid)
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
            t.Status = 2;
        }

        await _context.SaveChangesAsync();
    }
}