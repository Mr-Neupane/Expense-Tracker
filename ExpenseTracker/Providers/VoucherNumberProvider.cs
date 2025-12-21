using Dapper;
using ExpenseTracker.Data;


namespace ExpenseTracker.Providers;

public class VoucherNumberProvider
{
    private readonly ApplicationDbContext _dbContext;

    public VoucherNumberProvider(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public static async Task<string> GetVoucherNumber()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var getvoucher = @"select concat('AV0000', (count(1)+1)) Voucherno
from accounting.transactions";
        var voucherno = await conn.QueryFirstAsync<string>(getvoucher);
        return voucherno;
    }
}