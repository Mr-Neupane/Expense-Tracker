using Dapper;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class LedgerController : Controller
{
    [HttpGet]
    public async Task<IActionResult> CreateLedger()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateLedger(LedgerVm l)
    {
        try
        {
            var conn = DapperConnectionProvider.GetConnection();

            var newLedger = @"
                                 Insert into accounting.Ledger (ParentId, LedgerName, RecStatus, Status,RecById) 
                                 values (@parentId, @ledgerName, @recStatus, @status, @recById)
                              ON CONFLICT (LedgerName) DO NOTHING;  ";
            await conn.ExecuteAsync(newLedger,
                new { ParentId = l.ParentId, Ledgername = l.LedgerName, RecStatus = 'A', Status = 1, RecById = 1 });
            conn.Close();
            return RedirectToAction("CreateLedger");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<IActionResult> LedgerReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var result =
            await conn.QueryAsync(
                "select c.name, l.ledgername as parentname,ls.code ledgercode, ls.ledgername ledgername, ls.status,username from accounting.ledger l join accounting.ledger ls on l.id = ls.subparentid join accounting.coa c on l.parentid = c.id join users u on u.id = ls.recbyid order by ls.id;");
        return View(result);
    }
}