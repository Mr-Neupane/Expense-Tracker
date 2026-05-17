using System.Transactions;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IToastNotification _toastNotification;

    public AccountController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IToastNotification toastNotification)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var user = await _userManager.FindByNameAsync(vm.Username);
                if (user == null)
                {
                    _toastNotification.AddErrorToastMessage("Invalid username or password.");
                    return View(vm);
                }

                var result = await _signInManager.PasswordSignInAsync(user, vm.Password, false, false);

                if (!result.Succeeded)
                {
                    _toastNotification.AddErrorToastMessage("Invalid username or password.");
                    return View(vm);
                }

                scope.Complete();
            }

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
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await _signInManager.SignOutAsync();
            scope.Complete();
        }

        _toastNotification.AddSuccessToastMessage("Logged out successfully.");
        return RedirectToAction("Login");
    }
}
