using Dapper;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
        try
        {
            var con = DapperConnectionProvider.GetConnection();
            var newbank =
                @"INSERT INTO bank.bank (bankname, accountnumber,bankcontactnumber, bankaddress, accountopendate, recstatus,recdate, status)
    VALUES (@bankname, @accountnumber,@bankcontactnumber, @bankaddress, @accountopendate, @recstatus,@recdate, @status)
    ON CONFLICT (bankname,accountnumber) DO NOTHING;";

            await con.ExecuteAsync(newbank,
                new
                {
                    bankname = vm.BankName,
                    accountnumber = vm.AccountNumber,
                    bankcontactnumber = vm.BankContact,
                    bankaddress = vm.BankAddress,
                    accountopendate = vm.AccountOpenDate,
                    recstatus = vm.RecStatus,
                    recdate = DateTime.Now,
                    status = vm.Status,
                });
            con.Close();

            return RedirectToAction("CreateBank");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> BankReport()
    {
        var con = DapperConnectionProvider.GetConnection();
        var bank =@"SELECT * FROM bank.bank";
        var sql = await con.QueryAsync(bank);
        return View(sql);
    }
}