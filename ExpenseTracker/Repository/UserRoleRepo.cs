using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class UserRoleRepo : GenericRepository<UserRole>, IUserRoleRepo
{
    public UserRoleRepo(ApplicationDbContext context) : base(context)
    {
    }
}
