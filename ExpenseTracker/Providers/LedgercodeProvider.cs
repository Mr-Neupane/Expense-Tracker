using Dapper;
using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var con = DapperConnectionProvider.GetConnection();
        var code =
            @"select concat(code, '.', cn) from (select count(1) + 1 cn from accounting.ledger l where subparentid = @subparentid) d 
    cross join lateral ( select code from accounting.ledger a where a.id =@subparentid) c;";

        var ledgercode = await con.QueryFirstAsync<string>(code, new
        {
            subparentid = subparentid
        });
        return ledgercode;
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