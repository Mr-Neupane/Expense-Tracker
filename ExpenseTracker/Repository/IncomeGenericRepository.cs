using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class IncomeGenericRepository : GenericRepository<Income>, IIncomeGenericRepository
{
    public IncomeGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
