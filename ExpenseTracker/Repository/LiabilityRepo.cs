using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class LiabilityRepo : GenericRepository<Liability>, ILiabilityRepo
{
    public LiabilityRepo(ApplicationDbContext context) : base(context)
    {
    }
}
