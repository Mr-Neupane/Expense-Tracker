using Dapper;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class BankTransactionController : Controller
{
    [HttpGet]
    public IActionResult BankDepositandWithdraw()
    {
        return View();
    }
    
   [HttpPost]
    public async Task<IActionResult> BankDepositandWithdraw(BankTransactionVm vm)
    {
        return View();
    }
    
    [HttpGet]
    public JsonResult GetBanks()
    {
        var dbConnection = DapperConnectionProvider.GetConnection();
        string query = "SELECT id, bankname FROM bank.bank"; 
        var banks = dbConnection.Query(query).Select(b => new 
        {
            Id = b.id,
            bankname = b.bankname
        }).ToList();
        return Json(banks);
    }
}