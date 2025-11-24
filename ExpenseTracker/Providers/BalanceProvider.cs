using Dapper;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Providers;

public class BalanceProvider
{
    public static async Task<decimal> GetLedgerBalance(int ledgerid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var closingbalance = @"With LiabandIncome as (select sum(cramount) - sum(dramount) RemBalance, t.ledgerid
                       from accounting.coa c
                                join accounting.ledger l on l.parentid = c.id
                                join accounting.ledger ls on ls.subparentid = l.id
                                join accounting.transactiondetails t on ls.id = t.ledgerid
                       where c.id in (2, 3)
                         and t.status = 1
                       group by ledgerid),
     AssetsandExpense as (select sum(dramount) - sum(cramount) RemBalance, t.ledgerid
                          from accounting.coa c
                                   join accounting.ledger l on l.parentid = c.id
                                   join accounting.ledger ls on ls.subparentid = l.id
                                   join accounting.transactiondetails t on ls.id = t.ledgerid
                          where c.id in (1, 4)
                            and t.status = 1
                          group by t.ledgerid),
     FinalData as (select *
                   from AssetsandExpense
                   union all
                   select *
                   from LiabandIncome)
select RemBalance from FinalData where ledgerid=@ledgerid";

        var balance = await conn.QueryFirstOrDefaultAsync<decimal?>(closingbalance, new
        {
            ledgerid
        });
        return balance ?? 0;
    }
}