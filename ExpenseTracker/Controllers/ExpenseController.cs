using Dapper;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class ExpenseController : Controller
{
    [HttpGet]
    public IActionResult RecordExpense()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RecordExpense(ExpenseVm vm)
    {
        return View();
    }

    public JsonResult GetExpenseLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT ls.ledgername,ls.id as ledgerid
FROM accounting.ledger l
         join accounting.ledger ls on ls.subparentid = l.id
         join accounting.coa c on l.parentid = c.id where c.name='Expenses' ";

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }
    
    public JsonResult GetCashBankLedgers()
    {
        using (var conn = DapperConnectionProvider.GetConnection())
        {
            string sql = @"SELECT ls.ledgername,ls.id as ledgerid
FROM accounting.ledger ls where ls.subparentid in (-1,-1)";

            var list = conn.Query(sql).ToList();
            return Json(list);
        }
    }
}