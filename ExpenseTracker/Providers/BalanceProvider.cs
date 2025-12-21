using System.Dynamic;
using Dapper;

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
        conn.Close();
        return balance ?? 0;
    }

    public static async Task<LedgerStatementDto> GetLedgerOpeningandCosingBalance(int ledgerid, DateTime datefrom,
        DateTime dateto)
    {
        var fromdate = await DateHelper.GetEnglishDate(datefrom);
        var todate = dateto == null ? DateTime.Now : await DateHelper.GetEnglishDate(dateto);

        var conn = DapperConnectionProvider.GetConnection();
        var query = @"
With Opening as (select ledger_id,
                               case
                                   when rem_amount < 0 then
                                       rem_amount * -1
                                   else rem_amount end                          as OpeningBalance,
                               case when rem_amount < 0 then 'Cr' else 'Dr' end as drcr
                        from (select sum(dr_amount) - sum(cr_amount) rem_amount, ledger_id
                              from accounting.transaction_details td
                                       join accounting.transactions t on td.transaction_id = t.id
                              where cast(txn_date as date) <= @datefrom
                              group by ledger_id) d),
     Closing as (select ledger_id,
                               case
                                   when rem_amount < 0 then
                                       rem_amount * -1
                                   else rem_amount end                          as ClosingBalance,
                               case when rem_amount < 0 then 'Cr' else 'Dr' end as cdrcr
                        from (select sum(dr_amount) - sum(cr_amount) rem_amount, ledger_id
                              from accounting.transaction_details td
                                       join accounting.transactions t on td.transaction_id = t.id
                              where cast(txn_date as date) <= @dateto
                              group by ledger_id) d)

select 
       coalesce(OpeningBalance, 0)         OpeningBalance,
       coalesce(o.ledger_id, c.ledger_id) ledgerid,
       coalesce(drcr, cdrcr)              drcr,c.*
from Opening o
         right join Closing c on o.ledger_id = c.ledger_id
where c.ledger_id=@ledgerid;
";
        var res = await conn.QueryFirstOrDefaultAsync<LedgerStatementDto>(query, new
        {
            LedgerId = ledgerid,
            datefrom = fromdate,
            dateto = todate
        });

        var rep = new LedgerStatementDto
        {
            LedgerId = res.LedgerId,
            OpeningBalance = res.OpeningBalance,
            ClosingBalance = res.ClosingBalance,
        };
        conn.Close();
        return rep;
    }
}