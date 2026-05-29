using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class CoaGenericRepository : GenericRepository<Coa>, ICoaGenericRepository
{
    public CoaGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
