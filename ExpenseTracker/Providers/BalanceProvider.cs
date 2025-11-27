using Dapper;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Providers;

public class BalanceProvider
{
    public static async Task<decimal> GetLedgerBalance(int ledgerid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var closingbalance = @"With LiabandIncome as (select sum(cr_amount) - sum(dr_amount) RemBalance, t.ledger_id
                       from accounting.coa c
                                join accounting.ledger l on l.parentid = c.id
                                join accounting.ledger ls on ls.subparentid = l.id
                                join accounting.transaction_details t on ls.id = t.ledger_id
                       where c.id in (2, 3)
                         and t.status = 1
                       group by ledger_id),
     AssetsandExpense as (select sum(dr_amount) - sum(cr_amount) RemBalance, t.ledger_id
                          from accounting.coa c
                                   join accounting.ledger l on l.parentid = c.id
                                   join accounting.ledger ls on ls.subparentid = l.id
                                   join accounting.transaction_details t on ls.id = t.ledger_id
                          where c.id in (1, 4)
                            and t.status = 1
                          group by t.ledger_id),
     FinalData as (select *
                   from AssetsandExpense
                   union all
                   select *
                   from LiabandIncome)
select RemBalance from FinalData where ledger_id=@ledgerid";

        var balance = await conn.QueryFirstOrDefaultAsync<decimal?>(closingbalance, new
        {
            ledgerid
        });
        return balance ?? 0;
    }
}