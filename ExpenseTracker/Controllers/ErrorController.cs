using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("/Error")]
    public IActionResult Index(string? errorCode)
    {
        var code = errorCode ?? HttpContext.Items["ErrorCode"] as string;
        return View(new ErrorViewModel { ErrorCode = code });
    }
}
