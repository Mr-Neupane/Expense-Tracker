using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class IncomeRepo : GenericRepository<Income>, IIncomeRepo
{
    public IncomeRepo(ApplicationDbContext context) : base(context)
    {
    }
}
