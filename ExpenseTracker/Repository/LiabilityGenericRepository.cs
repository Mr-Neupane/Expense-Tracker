using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class LiabilityGenericRepository : GenericRepository<Liability>, ILiabilityGenericRepository
{
    public LiabilityGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
