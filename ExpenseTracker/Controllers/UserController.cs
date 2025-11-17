using Dapper;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class UserController : Controller
{
    [HttpGet]
    public async Task<IActionResult> AddUser()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(UserVm vm)
    {
        try
        {
            using var conn = DapperConnectionProvider.GetConnection();
            

            var insertQuery = @"
INSERT INTO users (username, password)
VALUES (@Username, @Password)
ON CONFLICT (username) DO NOTHING;
";


            await conn.ExecuteAsync(insertQuery, new
            {
                Username = vm.Username,
                Password = vm.Password
            });
            conn.Close();

            return View(vm);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}