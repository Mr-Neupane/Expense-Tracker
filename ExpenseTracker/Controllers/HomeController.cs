using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
