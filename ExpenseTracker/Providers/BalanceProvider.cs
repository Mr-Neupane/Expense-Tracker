using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;
using TestApplication.ViewModels;

namespace ExpenseTracker.Providers;

public class IBalanceProvider
{
    private readonly ILedgerGenericRepository _ledgerGenericRepo;
    private readonly ICoaGenericRepository _coaGenericRepo;
    private readonly ITransactionDetailGenericRepository _txnDetailGenericRepo;
    private readonly ITransactionGenericRepository _txnRepo;

    public IBalanceProvider(ILedgerGenericRepository ledgerGenericRepo, ICoaGenericRepository coaGenericRepo,
        ITransactionDetailGenericRepository txnDetailGenericRepo, ITransactionGenericRepository txnRepo)
    {
        _ledgerGenericRepo = ledgerGenericRepo;
        _coaGenericRepo = coaGenericRepo;
        _txnDetailGenericRepo = txnDetailGenericRepo;
        _txnRepo = txnRepo;
    }

    public decimal GetLedgerBalance(int ledgerId)
    {
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var tdQuery = _txnDetailGenericRepo.GetBaseQueryable();
        var tQuery = _txnRepo.GetBaseQueryable();

        var ledgerTransaction =
             (from l in lQuery
                    join pl in lQuery on l.SubParentId equals pl.Id
                    join c in cQuery on pl.ParentId equals c.Id
                    join td in tdQuery on l.Id equals td.LedgerId
                    join t in tQuery on td.TransactionId equals t.Id
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
            var first = ledgerTransaction.First();
            var totalDr = ledgerTransaction.Sum(x => x.DrAmount);
            var totalCr = ledgerTransaction.Sum(x => x.CrAmount);
            var isLiabilityOrIncome = first.Id == 2 || first.Id == 3;
            var balance = isLiabilityOrIncome ? totalCr - totalDr : totalDr - totalCr;
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
        var toDate = dateTo == default ? DateTime.Now : dateTo;

        var tdQuery = _txnDetailGenericRepo.GetBaseQueryable();
        var tQuery = _txnRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();
        var cQuery = _coaGenericRepo.GetBaseQueryable();

        var openingBalance = await (from td in tdQuery
                join t in tQuery on td.TransactionId equals t.Id
                join l in lQuery on td.LedgerId equals l.Id
                join pl in lQuery on l.SubParentId equals pl.Id
                join c in cQuery on pl.ParentId equals c.Id
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

        var closingBalance = await (from td in tdQuery
                join t in tQuery on td.TransactionId equals t.Id
                join l in lQuery on td.LedgerId equals l.Id
                join pl in lQuery on l.SubParentId equals pl.Id
                join c in cQuery on pl.ParentId equals c.Id
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
            LedgerStatements = new List<LedgerStatement>(),
        };
        return rep;
    }
}
