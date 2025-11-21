using Dapper;
using ExpenseTracker.Controllers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Providers;

public class LedgerCode : Controller
{
    public static async Task<string> GetBankLedgercode()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var bankcode =
            @"select concat(code,'.',cn) from (select count(1) + 1 cn from accounting.ledger l where subparentid = -2) d cross join lateral ( select code from accounting.ledger a where a.id = -2) c;";
        var finalcode = await conn.QuerySingleAsync<string>(bankcode);
        return finalcode;
    }

    public static async Task<string> GetLedgerCode(LedgerVm vm)
    {
        var con = DapperConnectionProvider.GetConnection();
        var code =
            @"select concat(code, '.', cn) from (select count(1) + 1 cn from accounting.ledger l where subparentid = @subparentid) d cross join lateral ( select code from accounting.ledger a where a.id =@subparentid) c;";

        var ledgercode = await con.QueryFirstAsync<string>(code, new
        {
            subparentid = vm.SubParentId
        });
        return ledgercode;
    }

    public static async Task<int> GetBankLedgerId(BankTransactionVm vm)
    {
        var con = DapperConnectionProvider.GetConnection();
        var bankledgerid = @"select ledgerid from bank.bank b where b.id = @bankId";
        var ledgerid = await con.QueryFirstAsync(bankledgerid, new { bankId = vm.BankId });
        return 1;
    }
}