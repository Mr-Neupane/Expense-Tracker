using ExpenseTracker.Models;

namespace ExpenseTracker.Providers.Interfaces;

public interface ICurrentUserProvider
{
    bool IsLoggedIn();
    Task<AppUser?> GetCurrentUser();
    int? GetCurrentUserId();
}
