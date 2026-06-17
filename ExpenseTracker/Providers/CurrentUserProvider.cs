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

    public bool IsLoggedIn()
        => GetCurrentUserId() != null;

    public async Task<User> GetCurrentUser()
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return null;

        return await _userRepo.SingleAsync(u => u.Id == currentUserId.Value);
    }

    public int? GetCurrentUserId()
    {
        var userId = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return Convert.ToInt32(userId);
    }
}
