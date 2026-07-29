using ExpenseTracker.Models;

namespace ExpenseTracker.Providers.Interfaces;

public interface ICurrentUserProvider
{
    // bool IsLoggedIn();
    Task<User> GetCurrentUser();
    int GetCurrentUserId();
}
