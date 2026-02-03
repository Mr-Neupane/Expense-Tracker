using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApplication.Enums;


namespace ExpenseTracker.Providers;

public class DropdownProvider : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IBalanceProvider _balanceProvider;

    public DropdownProvider(ApplicationDbContext context, IBalanceProvider balanceProvider)
    {
        _context = context;
        _balanceProvider = balanceProvider;
    }

    [HttpGet]
    public List<DropdownListDto> GetAllBanks()
    {
        var banks = _context.Banks.Select(x => new DropdownListDto
        {
            Id = x.Id,
            Name = x.BankName
        }).ToList();
        return banks;
    }

    public List<DropdownListDto> GetExpenseLedgers()
    {
        var expLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.ParentId
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where c.Name == "Expenses" && ls.Status == Status.Active.ToInt()
                select new DropdownListDto()
                {
                    Name = ls.LedgerName,
                    Id = ls.Id
                }
            ).ToList();
        return expLedger;
    }

    public JsonResult GetLiabilityLedgers()
    {
        var liabilityLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.ParentId
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where c.Name == "Liabilities"
                select new
                {
                    ledgername = ls.LedgerName,
                    id = ls.Id
                }
            ).ToList();
        return Json(liabilityLedger);
    }

    public List<DropdownListDto> GetCashBankLedgers()
    {
        var cashAndBankLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.ParentId
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where (ls.SubParentId == -1 || ls.SubParentId == -2)
                select new DropdownListDto
                {
                    Name = ls.LedgerName,
                    Id = ls.Id
                }
            ).ToList();
        return cashAndBankLedger;
    }

    public List<DropdownListDto> GetIncomeLedgers()
    {
        var incomeLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.ParentId
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where c.Name == "Income"
                select new DropdownListDto()
                {
                    Name = ls.LedgerName,
                    Id = ls.Id
                }
            ).ToList();
        return incomeLedger;
    }

    public JsonResult GetLedgers()
    {
        var ledgers = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.ParentId
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                select new LedgerInfoForJvDto()
                {
                    LedgerBalance = 0,
                    LedgerName = string.Concat(c.Name, " > ", ls.LedgerName),
                    Code = ls.Code,
                    LedgerId = ls.Id
                }
                // select new
                // {
                //     coaname = l.LedgerName,
                //     ledgername = ls.LedgerName,
                //     code = ls.Code,
                //     id = ls.Id,
                //     balance = 0
                // }
            ).ToList();
        foreach (var l in ledgers)
        {
            var bs = _balanceProvider.GetLedgerBalance(l.LedgerId);
            l.LedgerBalance = bs;
        }

        return Json(ledgers);
    }

    public async Task<List<string>> GetTransactionTypeAsync()
    {
        var transactions = await _context.AccountingTransaction.ToListAsync();

        var txnType = transactions.Select(t => t.Type).Distinct().ToList();
        return txnType;
    }

    public async Task<List<LedgerInfoForJvDto>> GetLedgerInfoForJv()
    {
        var ledgers = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.ParentId
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                select new LedgerInfoForJvDto()
                {
                    LedgerName = string.Concat(l.LedgerName, " > ", ls.LedgerName),
                    Code = ls.Code,
                    LedgerId = ls.Id
                }
            ).ToList();

        foreach (var ledger in ledgers)
        {
            var bs = _balanceProvider.GetLedgerBalance(ledger.LedgerId);
            ledger.LedgerBalance = bs;
        }

        return ledgers;
    }
}