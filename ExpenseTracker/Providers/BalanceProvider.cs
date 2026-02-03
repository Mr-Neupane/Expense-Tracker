using Dapper;
using ExpenseTracker.Data;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;

namespace ExpenseTracker.Providers;

public class IBalanceProvider
{
    private readonly ApplicationDbContext _context;

    public IBalanceProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public decimal GetLedgerBalance(int ledgerId)
    {
        var ledgerTransaction =
             (from l in _context.Ledgers
                    join pl in _context.Ledgers on l.SubParentId equals pl.Id
                    join c in _context.CoaLedger on pl.ParentId equals c.Id
                    join td in _context.TransactionDetails on l.Id equals td.LedgerId
                    join t in _context.AccountingTransaction on td.TransactionId equals t.Id
                    where t.Status == Status.Active.ToInt() && td.Status == Status.Active.ToInt()
                                                            && td.LedgerId == ledgerId
                    select new
                    {
                        c.Name,
                        c.Id,
                        td.CrAmount,
                        td.DrAmount,
                    }
                ).ToList();
        if (ledgerTransaction.Count > 0)
        {
            var closingBalance = ledgerTransaction.Select(x => new
            {
                CoaName = ledgerTransaction.Select(x => x.Name),
                CoaId = ledgerTransaction.Select(x => x.Id),
                TotalDr = ledgerTransaction.Sum(x => x.DrAmount),
                TotalCr = ledgerTransaction.Sum(x => x.CrAmount),
                RemBalnce = ledgerTransaction.Select(x => x.Id == 2 || x.Id == 3) != null
                    ? ledgerTransaction.Sum(x => x.CrAmount) - ledgerTransaction.Sum(x => x.DrAmount)
                    : ledgerTransaction.Sum(x => x.DrAmount) - ledgerTransaction.Sum(x => x.CrAmount)
            }).ToList();

            decimal balance = closingBalance.Select(x => x.RemBalnce).Sum();
            return balance;
        }
        else
        {
            return 0;
        }
    }

    public async Task<LedgerStatementDto> GetLedgerOpeningandCosingBalance(int ledgerId, DateTime dateFrom,
        DateTime dateTo)
    {
        var fromDate = dateFrom;
        var toDate = dateTo == null ? DateTime.Now : dateTo;
        var openingBalance = await (from td in _context.TransactionDetails
                join t in _context.AccountingTransaction on td.TransactionId equals t.Id
                join l in _context.Ledgers on td.LedgerId equals l.Id
                join pl in _context.Ledgers on l.SubParentId equals pl.Id
                join c in _context.CoaLedger on pl.ParentId equals c.Id
                where t.Status == Status.Active.ToInt() && td.Status == Status.Active.ToInt()
                                                        && t.TxnDate < fromDate.ToUniversalTime() &&
                                                        td.LedgerId == ledgerId
                select new
                {
                    CoaId = c.Id,
                    td.LedgerId,
                    td.DrAmount,
                    td.CrAmount
                }
            ).ToListAsync();
        var totalOpening = openingBalance.Select(x => new
        {
            ledgerId = x.LedgerId,
            TotalOpeningBalance = x.CoaId == 1 || x.CoaId == 4
                ? openingBalance.Sum(x => x.DrAmount) - openingBalance.Sum(x => x.CrAmount)
                : openingBalance.Sum(x => x.CrAmount) - openingBalance.Sum(x => x.DrAmount),
        }).FirstOrDefault();

        var closingBalance = await (from td in _context.TransactionDetails
                join t in _context.AccountingTransaction on td.TransactionId equals t.Id
                join l in _context.Ledgers on td.LedgerId equals l.Id
                join pl in _context.Ledgers on l.SubParentId equals pl.Id
                join c in _context.CoaLedger on pl.ParentId equals c.Id
                where t.Status == Status.Active.ToInt() && td.Status == Status.Active.ToInt() &&
                      td.LedgerId == ledgerId &&
                      t.TxnDate.Date <= toDate.ToUniversalTime()
                select new
                {
                    CoaId = c.Id,
                    td.LedgerId,
                    td.DrAmount,
                    td.CrAmount
                }
            ).ToListAsync();
        var totalClosing = closingBalance.Select(x => new
        {
            ledgerId = x.LedgerId,
            TotalClosingBalance = x.CoaId == 1 || x.CoaId == 4
                ? closingBalance.Sum(x => x.DrAmount) - closingBalance.Sum(x => x.CrAmount)
                : closingBalance.Sum(x => x.CrAmount) - closingBalance.Sum(x => x.DrAmount),
        }).FirstOrDefault();

        var openingAmount = totalOpening != null ? totalOpening.TotalOpeningBalance : 0;
        var closingAmount = totalClosing != null ? totalClosing.TotalClosingBalance : 0;

        var rep = new LedgerStatementDto
        {
            LedgerId = ledgerId,
            DateFrom = fromDate,
            DateTo = toDate,
            OpeningBalance = openingAmount,
            ClosingBalance = closingAmount,
            LedgerStatements = null,
        };
        return rep;
    }
}