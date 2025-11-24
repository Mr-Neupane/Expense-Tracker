using Dapper;
using Microsoft.AspNetCore.Mvc;


namespace ExpenseTracker.Providers;

public class DropdownProvider : Controller
{
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
}