using Dapper;


namespace ExpenseTracker.Providers;

public class VoucherNumberProvider
{
    public static async Task<string> GetVoucherNumber()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var getvoucher = @"select concat('AV0000', (count(1)+1)) Voucherno
from accounting.transactions";
        var voucherno = await conn.QueryFirstAsync<string>(getvoucher);
        return voucherno;
    }
    
}