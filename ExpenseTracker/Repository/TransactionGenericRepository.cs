using ExpenseTracker.Data;

namespace ExpenseTracker.Repository;

public class TransactionGenericRepository : GenericRepository<Models.Transaction>, ITransactionGenericRepository
{
    public TransactionGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
