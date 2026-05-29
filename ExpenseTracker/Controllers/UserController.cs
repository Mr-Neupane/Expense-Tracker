<<<<<<< HEAD
using ExpenseTracker.Constants;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
=======
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Mvc;
>>>>>>> main
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace ExpenseTracker.Controllers;

public class UserController : Controller
{
<<<<<<< HEAD
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IToastNotification _toastNotification;

    public UserController(
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        IToastNotification toastNotification)
    {
        _userManager = userManager;
        _context = context;
=======
    private readonly IUserGenericRepository _userGenericRepo;
    private readonly IUow _uow;
    private readonly IToastNotification _toastNotification;

    public UserController(IUserGenericRepository userGenericRepo, IUow uow, IToastNotification toastNotification)
    {
        _userGenericRepo = userGenericRepo;
        _uow = uow;
>>>>>>> main
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
<<<<<<< HEAD
            var existingUser = await _userManager.FindByNameAsync(vm.Username);
            if (existingUser == null)
            {
                var nextId = await _context.Users.AnyAsync()
                    ? await _context.Users.MaxAsync(u => u.Id) + UserConstants.FirstUserId
                    : UserConstants.FirstUserId;

                var addUser = new AppUser
                {
                    Id = nextId,
                    UserName = vm.Username,
                    DisplayName = vm.Username
                };
                var result = await _userManager.CreateAsync(addUser, vm.Password);

                if (result.Succeeded)
                {
                    _toastNotification.AddSuccessToastMessage("User created successfully");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
=======
            var existingUser = await _userGenericRepo.SingleOrDefaultAsync(u => u.Username == vm.Username);
            if (existingUser == null)
            {
                var addUser = new User()
                {
                    Username = vm.Username,
                    Password = vm.Password
                };
                await _uow.AddAsync(addUser);
                await _uow.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("User created successfully");
>>>>>>> main
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
