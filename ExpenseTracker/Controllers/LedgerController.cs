using Dapper;
using ExpenseTracker.Providers;
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
    public async Task<IActionResult> CreateLedger(LedgerVm vm)
    {
        try
        {
            var conn = DapperConnectionProvider.GetConnection();
            var ledgercode = await LedgerCode.GetLedgerCode(vm);

            var subparentid = vm.SubParentId;
            var newLedger = @"
                                 INSERT INTO accounting.ledger ( parentid, ledgername, recstatus, status, recbyid, subparentid, code)
                                 values (@parentId, @ledgerName, @recStatus, @status, @recById,@subparentid,@code)
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
                "select c.name, l.ledgername as parentname,ls.code ledgercode, ls.ledgername ledgername, ls.status,username from accounting.ledger l join accounting.ledger ls on l.id = ls.subparentid join accounting.coa c on l.parentid = c.id join users u on u.id = ls.recbyid order by ls.id;");
        return View(result);
    }

    public IActionResult GetSubParents(int parentId)
    {
        var con = DapperConnectionProvider.GetConnection();

        string sql = @"SELECT Id, LedgerName, code
                           FROM accounting.ledger  
                           WHERE ParentId = @ParentId";
        var subParents = con.Query(sql, new { ParentId = parentId }).ToList();

        return Json(subParents);
    }
}