using System.Diagnostics;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ExpenseTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public required ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

   

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}