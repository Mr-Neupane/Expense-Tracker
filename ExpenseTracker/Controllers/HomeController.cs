using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using TestApplication.Models;

namespace ExpenseTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async void SeededQuery()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var userQuery = @"
Create table if not exists users;
";
        await conn.ExecuteAsync(userQuery);
        var coaQuery = @"
Create table if not exists COA;
       await conn.ExecuteAsync(coaQuery);

";
    }
}