using Dapper;
using ExpenseTracker;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

public class BankService
{
    public static async Task<int> RecordBankTransaction(BankTransactionVm vm)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    var banktran =
                        @"INSERT INTO bank.banktransactions ( bank_id,txn_date,amount,type,remarks,rec_date,rec_by_id,rec_status,status,transaction_id)
                                    values (@bank_id, @txn_date, @amount, @type, @remarks, @rec_date,@rec_by_id,@recs_tatus,@status,@transaction_id) returning id";
                    var id = await con.QuerySingleAsync<int>(banktran, new
                    {
                        bank_id = vm.BankId,
                        txn_date = vm.TxnDate,
                        amount = vm.Amount,
                        type = vm.Type,
                        remarks = vm.Remarks,
                        rec_date = DateTime.Now,
                        rec_by_id = -1,
                        recs_tatus = vm.RecStatus,
                        status = vm.Status,
                        transaction_id = 0,
                    });
                    await txn.CommitAsync();
                    await con.CloseAsync();
                    return id;
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

    public static async Task UpdateTransactionDuringBankTransaction(int btid, int transactionid)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                var query = @"UPDATE bank.banktransactions SET transaction_id = @transactionid where id =@id";
                await con.ExecuteAsync(query, new { transactionid = transactionid, id = btid });
                await txn.CommitAsync();
                await con.CloseAsync();
            }
        }
    }

    public static async Task<List<dynamic>> GetBankTransactionReport()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var txnreport =
            await conn.QueryAsync(@"select b.id bankid,b.bankname,t.*,u.username
            from bank.banktransactions t
                join users u on u.id = t.rec_by_id
            join bank.bank b on b.id = bank_id where t.status=1");

        return txnreport.ToList();
    }

    public static async Task ReverseBankTransactionByAccTranID(int tranid)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    var query = @"update bank.banktransactions set status=2 where status=1 and transaction_id=@tranid";
                    await con.ExecuteAsync(query, new { tranid });

                    var transactionReverse =
                        @"update accounting.transactions set status=2 where status=1 and id=@tranid";
                    await con.ExecuteAsync(transactionReverse, new { tranid });
                    var detailReverse =
                        @"update accounting.transactiondetails set status=2 where status=1 and transactionid=@tranid";
                    await con.ExecuteAsync(detailReverse, new { tranid });
                    await txn.CommitAsync();
                    await con.CloseAsync();
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
}