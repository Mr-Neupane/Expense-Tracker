using ExpenseTracker.Manager.Interfaces;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Manager;

public class AuthManager : IAuthManager
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthManager(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task Login(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new Exception("Invalid username");
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if (!result.Succeeded)
        {
            throw new Exception("Username and password do not match");
        }
    }

    public async Task Logout()
    {
        await _signInManager.SignOutAsync();
    }
}
