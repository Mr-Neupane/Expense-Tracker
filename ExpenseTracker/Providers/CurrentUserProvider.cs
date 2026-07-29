using System.Security.Claims;
using ExpenseTracker.Models;
using ExpenseTracker.Providers.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Providers;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly UserManager<AppUser> _userManager;

    public CurrentUserProvider(IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
    }

    // public bool IsLoggedIn()
    //     => GetCurrentUserId() != null;

    public async Task<AppUser> GetCurrentUser()
    {
        var currentUserId = GetCurrentUserId();

        return await _userManager.FindByIdAsync(currentUserId.ToString())?? throw new Exception("User not found");
    }

    public int GetCurrentUserId()
    {
        var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) throw new Exception("User not found");
        return Convert.ToInt32(userId);
    }
}
