using ExpenseTracker.Models;
using ExpenseTracker.Repository;
using ExpenseTracker.UnitOfWork.Interfaces;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class UserController : Controller
{
    private readonly IUserRepo _userGenericRepo;
    private readonly IUow _uow;
    private readonly IToastNotification _toastNotification;

    public UserController(IUserRepo userGenericRepo, IUow uow, IToastNotification toastNotification)
    {
        _userGenericRepo = userGenericRepo;
        _uow = uow;
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
            var existingUser = await _userGenericRepo.SingleOrDefaultAsync(u => u.UserName == vm.Username);
            if (existingUser == null)
            {
                var addUser = new User()
                {
                    UserName = vm.Username,
                    Password = vm.Password
                };
                await _uow.AddAsync(addUser);
                await _uow.SaveChangesAsync();
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
