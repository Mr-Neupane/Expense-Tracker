using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models;

public class AppUser : IdentityUser<int>
{
    public string? DisplayName { get; set; }
}
