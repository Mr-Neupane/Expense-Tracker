using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
using TestApplication.ViewModels.Interface;

namespace ExpenseTracker.Services;

public class BankService : IBankService
{
    private readonly IUow _uow;
    private readonly IBankGenericRepository _bankGenericRepo;
    private readonly IBankTransactionGenericRepository _bankTxnRepo;
    private readonly IUserGenericRepository _userGenericRepo;

    public BankService(IUow uow, IBankGenericRepository bankGenericRepo,
        IBankTransactionGenericRepository bankTxnRepo, IUserGenericRepository userGenericRepo)
    {
        _uow = uow;
        _bankGenericRepo = bankGenericRepo;
        _bankTxnRepo = bankTxnRepo;
        _userGenericRepo = userGenericRepo;
    }


    public async Task<List<Bank>> BankReportAsync()
    {
        return await _bankGenericRepo.GetAsync(b => b.Status == Status.Active.ToInt());
    }

    public async Task EditBankAsync(BankDto dto)
    {
        var bank = await _bankGenericRepo.FindOrThrowAsync(dto.Id);
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

        await _uow.SaveChangesAsync();
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
        await _uow.AddAsync(banktransaction);
        await _uow.SaveChangesAsync();
        return banktransaction;
    }

    public async Task UpdateAccountingTransactionIdInBankTransactionAsync(int id, int transactionId)
    {
        var txn = await _bankTxnRepo.GetAsync(t => t.Id == id);
        foreach (var t in txn)
        {
            t.TransactionId = transactionId;
        }

        await _uow.SaveChangesAsync();
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
            AccountOpenDate = dto.AccountOpenDate,
            RecStatus = 'A',
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active.ToInt(),
            RecById = -1
        };

        await _uow.AddAsync(bank);
        await _uow.SaveChangesAsync();
        return bank;
    }

    public async Task UpdateRemainingBalanceInBankAsync(int bid)
    {
        var bankTxnQuery = _bankTxnRepo.GetBaseQueryable();
        var deposit = await bankTxnQuery
            .Where(t => t.Status == Status.Active.ToInt() && t.Type == "Deposit" && t.BankId == bid)
            .SumAsync(t => t.Amount);
        var withdraw = await bankTxnQuery
            .Where(t => t.Status == Status.Active.ToInt() && t.Type == "Withdraw" && t.BankId == bid)
            .SumAsync(t => t.Amount);
        var remBal = deposit - withdraw;
        var bank = await _bankGenericRepo.SingleOrDefaultAsync(b => b.Id == bid);
        if (bank == null)
        {
            throw new Exception("Cannot update remaining balance");
        }
        else
        {
            bank.RemainingBalance = remBal;
            await _uow.SaveChangesAsync();
        }
    }

    public async Task ReverseBankTransactionAsync(int id, int transactionId)
    {
        var bankTxnQuery = _bankTxnRepo.GetBaseQueryable();
        var txn = await bankTxnQuery
            .Where(t => t.Id == id && t.TransactionId == transactionId)
            .FirstOrDefaultAsync();

        if (txn == null)
        {
            throw new Exception("Transaction not found");
        }
        else
        {
            txn.Status = Status.Reversed.ToInt();
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<List<BankTransactionReportDto>> BankTransactionReportAsync()
    {
        var btQuery = _bankTxnRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();
        var bQuery = _bankGenericRepo.GetBaseQueryable();

        var res = await (from bt in btQuery
                join u in uQuery on bt.RecById equals u.Id
                join b in bQuery on bt.BankId equals b.Id
                where bt.Status == Status.Active.ToInt()
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
                    Username = u.Username,
                }
            ).ToListAsync();
        return res;
    }
}
