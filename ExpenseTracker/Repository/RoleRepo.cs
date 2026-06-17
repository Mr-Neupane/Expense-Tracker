using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class RoleRepo : GenericRepository<Role>, IRoleRepo
{
    public RoleRepo(ApplicationDbContext context) : base(context)
    {
    }
}
