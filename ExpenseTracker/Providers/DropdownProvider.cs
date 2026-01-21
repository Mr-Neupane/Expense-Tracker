using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ExpenseTracker.Providers;

public class DropdownProvider : Controller
{
    private readonly ApplicationDbContext _context;

    public DropdownProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public JsonResult GetBanks()
    {
        var banks = _context.Banks.ToList().Select(b => new
        {
            id = b.Id,
            bankname = b.BankName
        }).ToList();
        return Json(banks);
    }

    public JsonResult GetExpenseLedgers()
    {
        var expLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.Parentid
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where c.Name == "Expenses"
                select new
                {
                    ledgername = ls.Ledgername,
                    id = ls.Id
                }
            ).ToList();
        return Json(expLedger);
    }

    public JsonResult GetLiabilityLedgers()
    {
        var liabilityLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.Parentid
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where c.Name == "Liabilities"
                select new
                {
                    ledgername = ls.Ledgername,
                    id = ls.Id
                }
            ).ToList();
        return Json(liabilityLedger);
    }

    public JsonResult GetCashBankLedgers()
    {
        var cashAndBankLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.Parentid
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where (ls.SubParentId == -1 || ls.SubParentId == -2)
                select new
                {
                    ledgername = ls.Ledgername,
                    id = ls.Id
                }
            ).ToList();
        return Json(cashAndBankLedger);
    }

    public JsonResult GetIncomeLedgers()
    {
        var incomeLedger = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.Parentid
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                where c.Name == "Income"
                select new
                {
                    ledgername = ls.Ledgername,
                    id = ls.Id
                }
            ).ToList();
        return Json(incomeLedger);
    }

    public JsonResult GetLedgers()
    {
        var ledgers = (from c in _context.CoaLedger
                join l in _context.Ledgers on c.Id equals l.Parentid
                join ls in _context.Ledgers on l.Id equals ls.SubParentId
                select new
                {
                    coaname = l.Ledgername,
                    ledgername = ls.Ledgername,
                    id = ls.Id
                }
            ).ToList();
        return Json(ledgers);
    }

    public async Task<List<string>> GetTransactionTypeAsync()
    {
        var transactions = await _context.AccountingTransaction.ToListAsync();

        var txnType = transactions.Select(t => t.Type).Distinct().ToList();
        return txnType;
    }
}