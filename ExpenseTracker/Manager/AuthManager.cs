using System.Security.Claims;
using ExpenseTracker.Manager.Interfaces;
using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Manager;

public class AuthManager : IAuthManager
{
    private readonly IUserRepo _userRepo;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthManager(IUserRepo userRepo, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Login(LoginDto dto)
    {
        var user = await _userRepo.SingleOrDefaultAsync(u => u.Email.ToLower() == dto.UserEmail);
        if (user == null)
            throw new Exception("Invalid username");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new Exception("Username and password do not match");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    public async Task Logout()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
