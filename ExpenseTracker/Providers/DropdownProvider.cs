using Dapper;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;


namespace ExpenseTracker.Providers;

public class DropdownProvider : Controller
{
    private readonly ApplicationDbContext _context;

    public DropdownProvider(ApplicationDbContext dbContext)
    {
        _context = dbContext;
    }

    [HttpGet]
    public JsonResult GetBanks()
    {
        var dbConnection = DapperConnectionProvider.GetConnection();
        string query = "SELECT id, bankname FROM bank.bank where status=1";
        var banks = dbConnection.Query(query).Select(b => new
        {
            Id = b.id,
            bankname = b.bankname
        }).ToList();
        return Json(banks);
    }

    public JsonResult GetExpenseLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT ls.ledgername,ls.id 
FROM accounting.ledger l
         join accounting.ledger ls on ls.subparentid = l.id
         join accounting.coa c on l.parentid = c.id where c.name='Expenses' ";

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }

    public JsonResult GetLiabilityLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT ls.ledgername,ls.id 
FROM accounting.ledger l
         join accounting.ledger ls on ls.subparentid = l.id
         join accounting.coa c on l.parentid = c.id where c.name='Liabilities' ";

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }

    public JsonResult GetCashBankLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT ls.ledgername,ls.id 
FROM accounting.ledger ls where ls.subparentid in (-1,-2)";

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }

    public JsonResult GetIncomeLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT ls.ledgername,ls.id 
FROM accounting.ledger l
         join accounting.ledger ls on ls.subparentid = l.id
         join accounting.coa c on l.parentid = c.id where c.name='Income' ";

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }

    public JsonResult GetLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT l.ledgername coaname,ls.ledgername,ls.id ,ls.code
FROM accounting.ledger l
         join accounting.ledger ls on ls.subparentid = l.id
         join accounting.coa c on l.parentid = c.id";

            var assetsAndExpense = new[] { 1, 4 };
            var dp = (from td in _context.TransactionDetails
                    join t in _context.AccountingTransaction on td.TransactionId equals t.Id
                    join l in _context.Ledgers on td.LedgerId equals l.Id
                    join l2 in _context.Ledgers on l.SubParentId equals l2.Id
                    where t.Status == 1 && td.Status == 1
                    group td by new { td.LedgerId, l.Ledgername, coaname = l2.Ledgername, l2.Parentid, l2.Code }
                    into res
                    select new
                    {
                        id = res.Key,
                        ledgername = res.Key.Ledgername,
                        code = res.Key.Code,
                        coaname = res.Key.coaname,
                        TotalDr = res.Sum(dr => dr.DrAmount),
                        TotalCr = res.Sum(dr => dr.CrAmount),
                        balance = res.Sum(dr => dr.DrAmount) - res.Sum(cr => cr.CrAmount),
                        drcr = res.Sum(dr => dr.DrAmount) - res.Sum(cr => cr.CrAmount) < 0 ? "Cr" : "Dr",
                        test = assetsAndExpense.Contains(res.Key.Parentid)
                            ?
                            res.Sum(dr => dr.DrAmount) - res.Sum(cr => cr.CrAmount) < 0 ? "Cr" : "Dr"
                            : res.Sum(dr => dr.DrAmount) - res.Sum(cr => cr.CrAmount) < 0
                                ? "Dr"
                                : "Cr"
                    }
                );
            return Json(dp);

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }
}