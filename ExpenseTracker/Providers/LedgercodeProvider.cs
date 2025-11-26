using Dapper;
using ExpenseTracker.Controllers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Providers;

public class LedgerCode : Controller
{
    public static async Task<string> GetLedgerCode(int subparentid)
    {
        var con = DapperConnectionProvider.GetConnection();
        var code =
            @"select concat(code, '.', cn) from (select count(1) + 1 cn from accounting.ledger l where subparentid = @subparentid) d cross join lateral ( select code from accounting.ledger a where a.id =@subparentid) c;";

        var ledgercode = await con.QueryFirstAsync<string>(code, new
        {
            subparentid = subparentid
        });
        return ledgercode;
    }

    public static async Task<int> GetBankLedgerId(int bankid)
    {
        var con = DapperConnectionProvider.GetConnection();
        var bankledgerid = @"select ledgerid from bank.bank b where b.id = @bankId";
        var ledgerid = await con.QueryFirstAsync<int>(bankledgerid, new { bankId = bankid });
        return ledgerid;
    }

    public static async Task<int> GetBankId(int ledgerid)
    {
        var con = DapperConnectionProvider.GetConnection();
        var bankledgerid = @"select ledgerid from bank.bank b where b.ledgerid = @ledgerid";
        var bankid = await con.QueryFirstAsync<int>(bankledgerid, new { bankId = ledgerid });
        return bankid;
    }

  

    public static async Task<int?> ValidateLedgerCode(string ledgercode)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select 1 from accounting.ledger where code = @code";
        int? res = await conn.QueryFirstOrDefaultAsync<int?>(query, new
        {
            code = ledgercode
        });
        return res ?? 0;
    }
}