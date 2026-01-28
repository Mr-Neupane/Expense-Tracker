using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.ViewModels;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IToastNotification _toastNotification;

    public UserController(ApplicationDbContext context, IToastNotification toastNotification)
    {
        _context = context;
        _toastNotification = toastNotification;
    }

    [HttpGet]
    public async Task<IActionResult> AddUser()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(UserVm vm)
    {
        try
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == vm.Username);
            if (existingUser == null)
            {
                var addUser = new User()
                {
                    Username = vm.Username,
                    Password = vm.Password
                };
                await _context.Users.AddAsync(addUser);
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("User created successfully");
            }

            else
            {
                _toastNotification.AddErrorToastMessage($"User with {vm.Username.Trim()} username already exists");
            }

            return View(vm);
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage(e.Message);
            return View(vm);
        }
    }

    public async Task<IActionResult> UserReport()
    {
        return RedirectToAction("AddUser");
    }
}