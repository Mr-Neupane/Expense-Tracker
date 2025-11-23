using TestApplication.ViewModels;

namespace ExpenseTracker.ViewModels;

public class UserVm : BaseVm
{
    public string Username { get; set; }
    public string Password { get; set; }
}