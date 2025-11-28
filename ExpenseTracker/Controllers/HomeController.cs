using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using Npgsql;

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
[HttpPost]
    public static async Task NepaliDate()
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            string sqlfilepath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Migrations", "NepaliDate.sql");
            string query = System.IO.File.ReadAllText(sqlfilepath);

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}