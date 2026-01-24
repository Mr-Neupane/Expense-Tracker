using Dapper;
using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
using TestApplication.ViewModels;

namespace ExpenseTracker.Providers;

public class IProvider : Controller
{
    private readonly ApplicationDbContext _context;

    public IProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetLedgerCode(int? subparentid)
    {
        var parentLedger = await _context.Ledgers.Where(x => x.Id == subparentid).SingleOrDefaultAsync();
        if (parentLedger == null)
        {
            throw new Exception("Ledger code not found");
        }
        else
        {
            var newCode = await _context.Ledgers
                .Where(x => x.SubParentId == subparentid).CountAsync() + 1;
            var ledgerCode = string.Concat(parentLedger.Code, '.', newCode);
            return ledgerCode;
        }
    }

    public async Task<int> GetBankLedgerId(int bankid)
    {
        var bank = await _context.Banks.Where(x => x.Id == bankid).SingleOrDefaultAsync();
        return bank.LedgerId;
    }

    public async Task<bool> ValidateLedgerCode(string ledgercode)
    {
        var existing = await _context.Ledgers.Select(x => x.Code == ledgercode).FirstOrDefaultAsync();
        return existing;
    }

    public async Task<int> GetBankIdByLedgerId(int ledgerid)
    {
        var bankId = await _context.Banks.Where(x => x.LedgerId == ledgerid).SingleOrDefaultAsync();
        if (bankId.Id == null)
        {
            bankId.Id = 0;
        }

        return bankId.Id;
    }
}