using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
using TestApplication.Interface;
using TestApplication.ViewModels;

namespace ExpenseTracker.Services;

public class LedgerService : ILedgerService
{
    private readonly ApplicationDbContext _context;
    private readonly IProvider _provider;

    public LedgerService(ApplicationDbContext context, IProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<Ledger> AddLedgerAsync(LedgerDto dto)
    {
        var ledgerCode = dto.IsParent ? dto.Code : await _provider.GetLedgerCode(dto.SubParentId);
        var ledger = new Ledger
        {
            Parentid = dto.ParentId,
            Ledgername = dto.Name,
            RecStatus = 'A',
            Status = Status.Active.ToInt(),
            RecById = -1,
            Code = ledgerCode,
            SubParentId = dto.SubParentId,
        };
        await _context.Ledgers.AddAsync(ledger);
        await _context.SaveChangesAsync();
        return ledger;
    }

    public async Task EditLedgerAsync(EditLedgerDto dto)
    {
        var ledger = await _context.Ledgers.FindAsync(dto.LedgerId);
        if (dto.LedgerName != ledger?.Ledgername)
        {
            ledger.Ledgername = dto.LedgerName;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ParentLedgerReportDto>> GetParentLedgerReportAsync()
    {
        var res = await (from l in _context.Ledgers
                join c in _context.CoaLedger on l.Parentid equals c.Id
                join u in _context.Users on l.RecById equals u.Id
                select new ParentLedgerReportDto
                {
                    LedgerId = l.Id,
                    Status = l.Status,
                    LedgerCode = l.Code,
                    LedgerName = l.Ledgername,
                    UserName = u.Username,
                    ParentLedgerName = c.Name
                }
            ).ToListAsync();
        return res;
    }

    public async Task<List<LedgerReportDto>> GetLedgerReportAsync()
    {
        var res = await (from l in _context.Ledgers
                join pl in _context.Ledgers on l.SubParentId equals pl.Id
                join c in _context.CoaLedger on pl.Parentid equals c.Id
                join u in _context.Users on l.RecById equals u.Id
                where l.Status == Status.Active.ToInt()
                select new LedgerReportDto
                {
                    LedgerId = l.Id,
                    SubParentName = string.Concat(pl.Ledgername, " [", pl.Code, "]"),
                    SubParentId = pl.Id,
                    LedgerName = l.Ledgername,
                    Code = l.Code,
                    CoaName = c.Name,
                    Status = l.Status,
                    UserName = u.Username,
                }
            ).ToListAsync();
        return res;
    }

    public async Task<List<LedgerStatement>> GetLedgerStatementsAsync(LedgerStatementDto vm)
    {
        var report =
            await BalanceProvider.GetLedgerOpeningandCosingBalance(vm.LedgerId, vm.DateFrom, vm.DateTo);
        var data = await (from t in _context.AccountingTransaction
            join td in _context.TransactionDetails.Where(d => d.LedgerId == vm.LedgerId) on t.Id equals td
                .TransactionId
            join td2 in _context.TransactionDetails on td.TransactionId equals td2.TransactionId
            join l in _context.Ledgers on td2.LedgerId equals l.Id
            where td2.LedgerId != vm.LedgerId && t.Status == Status.Active.ToInt() &&
                  td.Status == Status.Active.ToInt() &&
                  t.TxnDate >= vm.DateFrom.ToUniversalTime() &&
                  t.TxnDate <= vm.DateTo.ToUniversalTime()
            group new { t, td2, td, l } by td.TransactionId
            into g
            select new
            {
                TransactionID = g.Key,
                LedgerId = g.Select(x => x.td2.LedgerId).ToList(),
                VoucherNo = g.Select(x => x.t.VoucherNo).First(),
                LedgerNames = g.Select(x => x.l.Ledgername).ToList(),
                DrAmount = g.Select(x => x.td.DrAmount).First(),
                CrAmount = g.Select(x => x.td.CrAmount).First(),
                TxnDate = g.Select(x => x.t.TxnDate).First(),
            }).ToListAsync();
        var statement = data.Select(d => new LedgerStatement
        {
            TransactionID = d.TransactionID,
            LedgerId = d.LedgerId,
            LedgerName = string.Join(", ",
                d.LedgerNames),
            DrAmount = d.DrAmount,
            VoucherNo = d.VoucherNo,
            CrAmount = d.CrAmount,
            TxnDate = d.TxnDate,
            OpeningBalance = report.OpeningBalance,
            ClosingBalance = report.ClosingBalance,
        }).ToList();
        vm.LedgerStatements = statement;
        return statement;
    }

    public async Task<bool> DeactivateLedgerAsync(int ledgerId)
    {
        var ledger = await _context.Ledgers.FindAsync(ledgerId);
        var validation =
            await (from l in _context.Ledgers
                    join t in _context.TransactionDetails on l.Id equals t.LedgerId
                    where t.LedgerId == ledgerId && t.Status == Status.Active.ToInt() && l.SubParentId != -2
                    select t)
                .ToListAsync();
        if (validation.Count == 0)
        {
            ledger.Status = Status.Reversed.ToInt();
            await _context.SaveChangesAsync();
            return true;
        }
        else
        {
            var drAmount = validation.Sum(x => x.DrAmount);
            var crAmount = validation.Sum(x => x.CrAmount);
            if (drAmount - crAmount == 0)
            {
                ledger.Status = Status.Active.ToInt();
                await _context.SaveChangesAsync();
                return true;
            }
        }

        return false;
    }

    public async Task ActivateLedgerAsync(int ledgerId)
    {
        var ledger = await _context.Ledgers.FindAsync(ledgerId);
        ledger.Status = Status.Active.ToInt();
        await _context.SaveChangesAsync();
    }
}