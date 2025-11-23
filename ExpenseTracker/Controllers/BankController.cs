using Dapper;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Providers;
using Npgsql;

namespace ExpenseTracker.Controllers;

public class BankController : Controller
{
    [HttpGet]
    public IActionResult CreateBank()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateBank(BankVm vm)
    {
        using (NpgsqlConnection con = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    var bankcode = await LedgerCode.GetBankLedgercode();
                    var bankledger =
                        @"INSERT INTO accounting.ledger ( parentid, ledgername, recstatus, status, recbyid, subparentid, code)
VALUES (@parentid, @ledgername, @recstatus, @status, @recbyid, @subparentid, @code) on conflict (ledgername) DO NOTHING returning id;";

                    int? parentid = null;
                    var lid = await con.QueryFirstAsync<int>(bankledger, new
                    {
                        parentid,
                        ledgername = vm.BankName,
                        recstatus = vm.RecStatus,
                        status = vm.Status,
                        recbyid = 1,
                        subparentid = -2,
                        code = bankcode
                    });

                    var newbank =
                        @"INSERT INTO bank.bank ( bankname, accountnumber, bankcontactnumber, ledgerid,remainingbalance, bankaddress, accountopendate,
                       recstatus, recdate, status, recbyid)
    VALUES (@bankname, @accountnumber,@bankcontactnumber,@ledgerid,@remainingbalance, @bankaddress, @accountopendate, @recstatus,@recdate, @status,@recbyid)
    ON CONFLICT (bankname,accountnumber) DO NOTHING;";

                    await con.ExecuteAsync(newbank,
                        new
                        {
                            bankname = vm.BankName,
                            accountnumber = vm.AccountNumber,
                            bankcontactnumber = vm.BankContact,
                            ledgerid = lid,
                            remainingbalance = 0,
                            bankaddress = vm.BankAddress,
                            accountopendate = vm.AccountOpenDate,
                            recstatus = vm.RecStatus,
                            recdate = DateTime.Now,
                            status = vm.Status,
                            recbyid = -1
                        });
                    await txn.CommitAsync();

                    return RedirectToAction("BankReport");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> BankReport()
    {
        var con = DapperConnectionProvider.GetConnection();
        var bank = @"SELECT * FROM bank.bank";
        var sql = await con.QueryAsync(bank);
        return View(sql);
    }
}