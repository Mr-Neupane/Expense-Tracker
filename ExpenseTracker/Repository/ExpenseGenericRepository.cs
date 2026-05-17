using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class ExpenseGenericRepository : GenericRepository<Expense>, IExpenseGenericRepository
{
    public ExpenseGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
