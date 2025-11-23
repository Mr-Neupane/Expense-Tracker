using Dapper;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;
using ExpenseTracker.Providers;
using Npgsql;

namespace ExpenseTracker.Controllers;

public class LedgerController : Controller
{
    [HttpGet]
    public async Task<IActionResult> CreateLedger()
    {
        return View();
    }

    [HttpPost]
    public static async Task<int> CreateLedger(LedgerVm vm)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var ledgercode = await LedgerCode.GetLedgerCode(vm.SubParentId);

                    var newLedger = @"
                                INSERT INTO accounting.ledger ( parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                                 values (@parentId, @ledgerName, @recStatus, @status, @recById, @code, @subparentid)
                              ON CONFLICT (ledgername, code) DO NOTHING returning id; ";
                    int? parentid = null;
                var ledgerid=    await conn.QueryFirstAsync<int>(newLedger,
                        new
                        {
                            parentid = parentid, Ledgername = vm.LedgerName, recstatus = vm.RecStatus,
                            status = vm.Status,
                            RecById = -1, subparentid = vm.SubParentId, code = ledgercode
                        });
                    await txn.CommitAsync();
                    await conn.CloseAsync();
                    return ledgerid;
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await conn.CloseAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public async Task<IActionResult> LedgerReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var result =
            await conn.QueryAsync(
                @"
select l.ledgername as subparentname, ls.ledgername, c.name, l.status, username
from accounting.ledger l
         join accounting.ledger ls on l.id = ls.subparentid
         join accounting.coa c on c.id = l.parentid
         join users u on u.id = l.recbyid;
");
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