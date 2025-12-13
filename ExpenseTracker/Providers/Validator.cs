using Dapper;

namespace ExpenseTracker.Providers;

public static class Validator
{
    public static async Task<int> ValidateBankTransaction(int transactionid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        int? res = await conn.QueryFirstOrDefaultAsync<int?>(@"select 1 from bank.banktransactions where transaction_id=@id",
            new { id = transactionid });
      
        return res ?? 0;
    }
}