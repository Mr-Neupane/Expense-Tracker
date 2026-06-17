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
            var loginDto = new LoginDto()
            {
                Password = vm.Password, UserEmail = vm.UserEmail.Trim().ToLower()
            };
            await _authManager.Login(loginDto);

            _toastNotification.AddSuccessToastMessage("Login successful.");
            return RedirectToAction("Index", "Home");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            return View();
        }
    }

    public async Task<IActionResult> Logout()
    {
        await _authManager.Logout();

        _toastNotification.AddSuccessToastMessage("Logged out successfully.");
        return RedirectToAction("Login");
    }
}