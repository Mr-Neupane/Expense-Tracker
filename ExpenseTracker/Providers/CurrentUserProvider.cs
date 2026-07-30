using System.Security.Claims;
using ExpenseTracker.Models;
using ExpenseTracker.Providers.Interfaces;
using ExpenseTracker.Repository;

namespace ExpenseTracker.Providers;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserRepo _userRepo;

    public CurrentUserProvider(IHttpContextAccessor contextAccessor, IUserRepo userRepo)
    {
        _contextAccessor = contextAccessor;
        _userRepo = userRepo;
    }

    // public bool IsLoggedIn()
    //     => GetCurrentUserId() != null;

    public async Task<User> GetCurrentUser()
    {
        var currentUserId = GetCurrentUserId();

        return await _userRepo.SingleAsync(u => u.Id == currentUserId);
    }

    public int GetCurrentUserId()
    {
        var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) throw new Exception("User not found");
        return Convert.ToInt32(userId);
    }

    public DateTime GetLoginDate()
    {
        var cookieValue = _contextAccessor.HttpContext?.Request.Cookies["LoginDate"];
        if (string.IsNullOrWhiteSpace(cookieValue)) throw new Exception("Cookie not found");
        return DateTime.Parse(cookieValue, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }
}
