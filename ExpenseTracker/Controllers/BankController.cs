using Dapper;
using ExpenseTracker.Providers;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
        using (NpgsqlConnection con = DapperConnectionProvider.GetConnection())
        {
            using (var txn = con.BeginTransaction())
            {
                try
                {
                    var newbank =
                        @"INSERT INTO bank.bank (bankname, accountnumber,bankcontactnumber, bankaddress, accountopendate, recstatus,recdate, status)
    VALUES (@bankname, @accountnumber,@bankcontactnumber, @bankaddress, @accountopendate, @recstatus,@recdate, @status)
    ON CONFLICT (bankname,accountnumber) DO NOTHING;";

                    int? ledgerid = null;
                    await con.ExecuteAsync(newbank,
                        new
                        {
                            bankname = vm.BankName,
                            accountnumber = vm.AccountNumber,
                            bankcontactnumber = vm.BankContact,
                            ledgerid,
                            bankaddress = vm.BankAddress,
                            accountopendate = vm.AccountOpenDate,
                            recstatus = vm.RecStatus,
                            recdate = DateTime.Now,
                            status = vm.Status,
                        });

                    var bankcode = await LedgerCode.GetBankLedgercode();
                    var bankledger =
                        @"INSERT INTO accounting.ledger ( parentid, ledgername, recstatus, status, recbyid, subparentid, code)
VALUES (@parentid, @ledgername, @recstatus, @status, @recbyid, @subparentid, @code) on conflict (ledgername) DO NOTHING;";

                    int? parentid = null;
                    await con.ExecuteAsync(bankledger, new
                    {
                        parentid,
                        ledgername = vm.BankName,
                        recstatus = vm.RecStatus,
                        status = vm.Status,
                        recbyid = 1,
                        subparentid = -2,
                        code = bankcode
                    });
                    
                    await txn.CommitAsync();

                    return RedirectToAction("BankReport");
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

    [HttpGet]
    public async Task<IActionResult> BankReport()
    {
        var con = DapperConnectionProvider.GetConnection();
        var bank = @"SELECT * FROM bank.bank";
        var sql = await con.QueryAsync(bank);
        return View(sql);
    }
}