using System.Runtime.InteropServices.ComTypes;
using Dapper;
using Npgsql;

namespace ExpenseTracker.Services;

public class VoucherService
{
    public static async Task ReverseAccountingTransactionById(int tranid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    var query = @"update accounting.transactions set status=2 where status =1 and id = @tranid";
                    await conn.ExecuteAsync(query, new
                    {
                        tranid
                    });
                    await ReverseTransactionDetailByTranID(tranid: tranid);
                    await transaction.CommitAsync();
                    await conn.CloseAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    await conn.CloseAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public static async Task ReverseTransactionDetailByTranID(int tranid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    conn.ExecuteAsync(
                        "update accounting.transactiondetails set status=2 where status =1 and id = @tranid",
                        new { tranid });
                    await txn.CommitAsync();
                    await conn.CloseAsync();
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