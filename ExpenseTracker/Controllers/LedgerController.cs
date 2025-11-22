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
                new
                {
                    ParentId = vm.ParentId, Ledgername = vm.LedgerName, RecStatus = vm.RecStatus, Status = vm.Status,
                    RecById = -1, subparentid = vm.SubParentId, code = ledgercode
                });
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
                "select l.ledgername, c.name, l.status, username from accounting.ledger l join accounting.coa c on c.id = l.parentid join users u on u.id = l.recbyid;");
        return View(result);
    }
}