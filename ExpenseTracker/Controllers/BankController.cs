using Dapper;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Providers;
using Npgsql;
using TestApplication.ViewModels;

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
                    int subparentId = -2;

                    DateTime accountopendate = await DateHelper.GetEnglishDate(vm.AccountOpenDate);
                    int lid = await LedgerController.NewLedger(new LedgerVm
                    {
                        Id = vm.Id,
                        SubParentId = subparentId,
                        ParentId = 0,
                        LedgerName = vm.BankName
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
                            accountopendate = accountopendate,
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