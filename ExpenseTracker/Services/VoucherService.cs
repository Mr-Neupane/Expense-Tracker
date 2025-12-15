using System.Runtime.InteropServices.ComTypes;
using Dapper;
using ExpenseTracker.Data;
using Npgsql;

namespace ExpenseTracker.Services;

public class VoucherService
{
    private readonly ApplicationDbContext _dbContext;

    public VoucherService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string GetNextJvVoucherNo()
    {
        var vouchernumber = _dbContext.AccountingTransaction
            .Where(x => x.VoucherNo.StartsWith("JV"))
            .Select(x => x.VoucherNo.Substring(2))
            .AsEnumerable()
            .Select(x => int.Parse(x))
            .DefaultIfEmpty(0)
            .Max();
        var nextVoucherNumber = "JV0000" + (vouchernumber + 1);
        return nextVoucherNumber;
    }
}