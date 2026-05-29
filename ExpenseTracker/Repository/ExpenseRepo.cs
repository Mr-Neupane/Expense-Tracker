using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class ExpenseRepo : GenericRepository<Expense>, IExpenseRepo
{
    public ExpenseRepo(ApplicationDbContext context) : base(context)
    {
    }
}
