using ExpenseTracker.Dtos;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;

namespace ExpenseTracker.Providers;

public class DropdownProvider
{
    private readonly ICoaGenericRepository _coaGenericRepo;
    private readonly ILedgerGenericRepository _ledgerGenericRepo;
    private readonly IBankGenericRepository _bankGenericRepo;
    private readonly ITransactionGenericRepository _txnRepo;
    private readonly IBalanceProvider _balanceProvider;
    private const int CashParentLedgerId = -1;
    private const int BankParentLedgerId = -2;

    public DropdownProvider(ICoaGenericRepository coaGenericRepo, ILedgerGenericRepository ledgerGenericRepo,
        IBankGenericRepository bankGenericRepo, ITransactionGenericRepository txnRepo, IBalanceProvider balanceProvider)
    {
        _coaGenericRepo = coaGenericRepo;
        _ledgerGenericRepo = ledgerGenericRepo;
        _bankGenericRepo = bankGenericRepo;
        _txnRepo = txnRepo;
        _balanceProvider = balanceProvider;
    }

    public List<DropdownListDto> GetAllBanks()
    {
        return _bankGenericRepo.GetBaseQueryable()
            .Select(x => new DropdownListDto { Id = x.Id, Name = x.BankName }).ToList();
    }

    public List<DropdownListDto> GetExpenseLedgers()
    {
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();

        return (from c in cQuery
                join l in lQuery on c.Id equals l.ParentId
                join ls in lQuery on l.Id equals ls.SubParentId
                where c.Name == "Expenses" && ls.Status == Status.Active.ToInt()
                select new DropdownListDto { Name = ls.LedgerName, Id = ls.Id }
            ).ToList();
    }

    public List<LedgerInfoForJvDto> GetLedgers()
    {
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();

        var ledgers = (from c in cQuery
                join l in lQuery on c.Id equals l.ParentId
                join ls in lQuery on l.Id equals ls.SubParentId
                select new LedgerInfoForJvDto
                {
                    LedgerBalance = 0,
                    LedgerName = string.Concat(l.LedgerName, " > ", ls.LedgerName),
                    Code = ls.Code,
                    LedgerId = ls.Id
                }
            ).ToList();
        foreach (var l in ledgers)
            l.LedgerBalance = _balanceProvider.GetLedgerBalance(l.LedgerId);

        return ledgers;
    }

    public List<DropdownListDto> GetCashBankLedgers()
    {
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();

        return (from c in cQuery
                join l in lQuery on c.Id equals l.ParentId
                join ls in lQuery on l.Id equals ls.SubParentId
                where ls.SubParentId == CashParentLedgerId || ls.SubParentId == BankParentLedgerId
                select new DropdownListDto { Name = ls.LedgerName, Id = ls.Id }
            ).ToList();
    }

    public List<DropdownListDto> GetIncomeLedgers()
    {
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();

        return (from c in cQuery
                join l in lQuery on c.Id equals l.ParentId
                join ls in lQuery on l.Id equals ls.SubParentId
                where c.Name == "Income"
                select new DropdownListDto { Name = ls.LedgerName, Id = ls.Id }
            ).ToList();
    }

    public List<DropdownListDto> GetLiabilityLedgers()
    {
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();

        return (from c in cQuery
                join l in lQuery on c.Id equals l.ParentId
                join ls in lQuery on l.Id equals ls.SubParentId
                where c.Name == "Liabilities"
                select new DropdownListDto { Name = ls.LedgerName, Id = ls.Id }
            ).ToList();
    }

    public async Task<List<string>> GetTransactionTypeAsync()
    {
        var transactions = await _txnRepo.GetAllAsync();
        return transactions.Select(t => t.Type).Distinct().ToList();
    }
}
