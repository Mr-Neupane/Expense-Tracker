using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class UserGenericRepository : GenericRepository<User>, IUserGenericRepository
{
    public UserGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
