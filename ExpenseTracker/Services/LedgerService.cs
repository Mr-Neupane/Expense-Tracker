using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using Microsoft.EntityFrameworkCore;
using TestApplication.Interface;
using TestApplication.ViewModels;

namespace ExpenseTracker.Services;

public class LedgerService : ILedgerService
{
    private readonly ApplicationDbContext _context;

    public LedgerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Ledger> AddLedgerAsync(LedgerDto dto)
    {
        var ledgercode = await LedgerCode.GetLedgerCode(dto.SubParentId);
        var ledger = new Ledger
        {
            Parentid = dto.ParentId,
            Ledgername = dto.Name,
            RecStatus = 'A',
            Status = 1,
            RecById = -1,
            Code = ledgercode,
            SubParentId = dto.SubParentId,
        };
        await _context.Ledgers.AddAsync(ledger);
        await _context.SaveChangesAsync();
        return ledger;
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
            where td2.LedgerId != vm.LedgerId && t.Status == 1 && td.Status == 1
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
            StatementDtos = new List<LedgerStatementDto>
            {
                new() { ClosingBalance = report.ClosingBalance },
                new() { OpeningBalance = report.OpeningBalance },
            },
        }).ToList();
        vm.LedgerStatements = statement;
        return statement;
    }
}