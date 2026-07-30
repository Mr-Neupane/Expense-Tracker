namespace ExpenseTracker.Manager.Interfaces;

public interface IAuthManager
{
    Task Login(LoginDto dto);
    Task Logout();
}

public class LoginDto
{
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string Password { get; set; }
    public DateTime LoginDate { get; set; }
}