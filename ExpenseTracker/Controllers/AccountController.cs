using ExpenseTracker.Manager.Interfaces;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class AccountController : Controller
{
    private readonly IAuthManager _authManager;
    private readonly IToastNotification _toastNotification;

    public AccountController(
        IAuthManager authManager,
        IToastNotification toastNotification)
    {
        _authManager = authManager;
        _toastNotification = toastNotification;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        try
        {
            await _authManager.Login(vm.Username, vm.Password);

            _toastNotification.AddSuccessToastMessage("Login successful.");
            return RedirectToAction("Index", "Home");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            return View(vm);
        }
    }

    public async Task<IActionResult> Logout()
    {
        await _authManager.Logout();

        _toastNotification.AddSuccessToastMessage("Logged out successfully.");
        return RedirectToAction("Login");
    }
}
