using Dapper;
using ExpenseTracker.Controllers;
using ExpenseTracker.Providers;
using Npgsql;
using NToastNotify;

namespace ExpenseTracker.Services;

public class ReverseService
{
    public static async Task ReverseBankTransactionByAccTranId(int tranid)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = await con.BeginTransactionAsync())
            {
                try
                {
                    var rep = await BankService.GetBankTransactionList(tranid);

                    if (rep.First().type == "Deposit")
                    {
                        var balance = await BalanceProvider.GetLedgerBalance((int)rep.First().ledgerid);
                        if (rep.First().amount > balance)
                        {
                            throw new Exception("Insufficient balance");
                        }
                    }

                    else
                    {
                        var query =
                            @"update bank.banktransactions set status=2 where status=1 and transaction_id=@tranid";
                        await con.ExecuteAsync(query, new { tranid });

                        var transactionReverse =
                            @"update accounting.transactions set status=2 where status=1 and id=@tranid";
                        await con.ExecuteAsync(transactionReverse, new { tranid });
                        var detailReverse =
                            @"update accounting.transaction_details set status=2 where status=1 and transaction_id=@tranid";
                        await con.ExecuteAsync(detailReverse, new { tranid });
                        await txn.CommitAsync();

                        await BankTransactionController.BankRemainingBalanceManager(rep.First().bank_id);
                        await con.CloseAsync();
                    }
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await con.CloseAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

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
                        await ReverseBankTransactionByAccTranId(tranid);
                    }

                    await txn.CommitAsync();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
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
                        await ReverseBankTransactionByAccTranId(transactionid);
                    }

                    await txn.CommitAsync();
                    await conn.CloseAsync();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await conn.CloseAsync();
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
                        await ReverseBankTransactionByAccTranId(transactionid);
                    }

                    await txn.CommitAsync();
                    await conn.CloseAsync();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    await conn.CloseAsync();
                }
            }
        }
    }
}