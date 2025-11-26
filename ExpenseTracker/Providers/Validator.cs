using Dapper;

namespace ExpenseTracker.Providers;

public class Validator
{
    public static async Task<int> ValidateBankTransaction(int ledgerid)
    {
        var con = DapperConnectionProvider.GetConnection();
        var query = @"select id from bank.bank where ledgerid= @ledgerid ";
        int? res = await con.QueryFirstAsync<int?>(query, new { ledgerid = ledgerid });
        return res ?? 0;
    }
}