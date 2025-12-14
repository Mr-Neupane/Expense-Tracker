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
    public async Task<IActionResult> CreateLedger(LedgerVm vm)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    await NewLedger(vm);
                    return RedirectToAction("LedgerReport");
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
select l.ledgername as subparentname, ls.ledgername,ls.code, c.name, l.status, username
from accounting.ledger l
         join accounting.ledger ls on l.id = ls.subparentid
         join accounting.coa c on c.id = l.parentid
         join users u on u.id = l.recbyid;
");
        return View(result);
    }

    [HttpGet]
    public IActionResult CreateParentLedger()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateParentLedger(ParentledgerVm vm)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                var validation = await LedgerCode.ValidateLedgerCode(vm.ParentCode);

                try
                {
                    if (validation == 1)
                    {
                        TempData["AlertMessage"] = "Ledger code already exists";
                        return RedirectToAction("CreateParentLedger");
                    }

                    int? subparentid = null;
                    var query = @"
INSERT INTO accounting.ledger ( parentid, ledgername, recstatus, status, recbyid, code, subparentid)
values (@parentid,@ledgername,@recstatus,@status,@recById, @code, @subparentid)";

                    await conn.ExecuteAsync(query, new
                    {
                        parentid = vm.ParentId,
                        ledgername = vm.ParentLedgerName,
                        recstatus = vm.RecStatus,
                        status = vm.Status,
                        recbyid = -1,
                        code = vm.ParentCode,
                        subparentid
                    });
                    await txn.CommitAsync();
                    await conn.CloseAsync();
                    return RedirectToAction("CreateParentLedger");
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

    [HttpGet]
    public async Task<IActionResult> ParentLedgerReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var result =
            @"select l.*,c.name,username from accounting.ledger l join accounting.coa c on c.id = l.parentid join users u on u.id = l.recbyid order by l.parentid";
        var res = await conn.QueryAsync(result);
        return View(res);
    }

    [HttpGet]
    public IActionResult LedgerStatement()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LedgerStatement(LedgerstatementVm vm)
    {
        var report = await BalanceProvider.GetLedgerOpeningandCosingBalance(vm.LedgerId, vm.DateFrom, vm.DateTo);

        return View();
    }

    public IActionResult GetSubParents(int parentId)
    {
        var con = DapperConnectionProvider.GetConnection();

        string sql = @"SELECT Id, LedgerName, code
                           FROM accounting.ledger  
                           WHERE ParentId = @ParentId and id not in (-2)";
        var subParents = con.Query(sql, new { ParentId = parentId }).ToList();

        return Json(subParents);
    }

    public static async Task<int> NewLedger(LedgerVm vm)
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
                    var ledgerid = await conn.QueryFirstAsync<int>(newLedger,
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
}