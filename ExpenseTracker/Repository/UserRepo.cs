using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class UserRepo : GenericRepository<User>, IUserRepo
{
    public UserRepo(ApplicationDbContext context) : base(context)
    {
    }
}
