using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;
using ExpenseTracker.ExtMethods;
using ExpenseTracker.ViewModels.Interface;

namespace ExpenseTracker.Services;

public class BankService : IBankService
{
    private readonly IUow _uow;
    private readonly IBankRepo _bankRepo;
    private readonly IBankTransactionRepo _bankTxnRepo;

    public BankService(IUow uow, IBankRepo bankRepo,
        IBankTransactionRepo bankTxnRepo)
    {
        _uow = uow;
        _bankRepo = bankRepo;
        _bankTxnRepo = bankTxnRepo;
    }
    

    public async Task EditBankAsync(BankDto dto)
    {
        var bank = await _bankRepo.FindOrThrowAsync(dto.Id);
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
        var bankTxn = new BankTransaction
        {
            BankId = dto.BankId,
            TxnDate = dto.TxnDate.ToUniversalTime(),
            Amount = dto.Amount,
            Type = dto.Type,
            Remarks = dto.Remarks,
            RecDate = DateTime.Now.ToUniversalTime(),
            RecById =dto.User.Id,
            RecStatus = RecordStatusConstants.Active,
            Status = Status.Active,
            TransactionId = 0
        };
        await _uow.AddAsync(bankTxn);
        await _uow.SaveChangesAsync();
        return bankTxn;
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
            AccountOpenDate = dto.AccountOpenDate.ToUniversalTime(),
            RecStatus = RecordStatusConstants.Active,
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active,
            RecById = dto.User.Id
        };

        await _uow.AddAsync(bank);
        await _uow.SaveChangesAsync();
        return bank;
    }

    public async Task UpdateRemainingBalanceInBankAsync(int bid)
    {
        var bankTxnQuery = _bankTxnRepo.GetBaseQueryable();
        var deposit = await bankTxnQuery
            .FilterActiveStatus()
            .Where(t => t.Type == TransactionTypeConstants.Deposit && t.BankId == bid)
            .SumAsync(t => t.Amount);
        var withdraw = await bankTxnQuery
            .FilterActiveStatus()
            .Where(t => t.Type == TransactionTypeConstants.Withdraw && t.BankId == bid)
            .SumAsync(t => t.Amount);
        var remBal = deposit - withdraw;
        var bank = await _bankRepo.SingleOrDefaultAsync(b => b.Id == bid);
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
            txn.Status = Status.Reversed;
            await _uow.SaveChangesAsync();
        }
    }

    
}
