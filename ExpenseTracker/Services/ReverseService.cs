using Dapper;
using ExpenseTracker.Controllers;
using ExpenseTracker.Providers;
using Npgsql;

namespace ExpenseTracker.Services;

public static class ReverseService
{
    public static async Task ReverseRecordedLiability(int liabid, int tranid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var incomerev = @"update accounting.liability set status=2 where status =1 and id = @liabid";
                    await conn.ExecuteAsync(incomerev, new { liabid });

                    var accrev =
                        @"update accounting.transactions set status=2 where status =1 and id = @tranid and type_id=@liabid";
                    await conn.ExecuteAsync(accrev, new { tranid, liabid });

                    var accdtlrev =
                        @"update accounting.transaction_details set status=2 where status =1 and transaction_id = @tranid";
                    await conn.ExecuteAsync(accdtlrev, new { tranid });

                    var isbanktran = await Validator.ValidateBankTransaction(tranid);
                    if (isbanktran != 0)
                    {
                        // await ReverseBankTransactionByAccTranId(tranid);
                    }

                    await txn.CommitAsync();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public static async Task ReverseIncome(int id, int transactionid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var mainupd = @"update accounting.income
                    set status=2
                    where id = @id;";

                    await conn.ExecuteAsync(mainupd, new { id });

                    var acctran = @"update accounting.transactions
                    set status=2 where 
                   id= @transactionid ;";

                    await conn.ExecuteAsync(acctran, new { transactionid });

                    var detail = @"update accounting.transaction_details
                    set status=2
                    where transaction_id= @transactionid ;";

                    await conn.ExecuteAsync(detail, new { transactionid });
                    var isbanktran = await Validator.ValidateBankTransaction(transactionid);
                    if (isbanktran != 0)
                    {
                        // await ReverseBankTransactionByAccTranId(transactionid);
                    }

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

    public static async Task ReverseExpense(int id, int transactionid)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var mainupd = @"update accounting.expenses
                    set status=2
                    where id = @id;";

                    await conn.ExecuteAsync(mainupd, new { id });

                    var acctran = @"update accounting.transactions
                    set status=2 where 
                   id= @transactionid ;";

                    await conn.ExecuteAsync(acctran, new { transactionid });

                    var detail = @"update accounting.transaction_details
                    set status=2
                    where transaction_id= @transactionid ;";

                    await conn.ExecuteAsync(detail, new { transactionid });

                    var isbanktran = await Validator.ValidateBankTransaction(transactionid);
                    if (isbanktran != 0)
                    {
                        // await ReverseBankTransactionByAccTranId(transactionid);
                    }

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