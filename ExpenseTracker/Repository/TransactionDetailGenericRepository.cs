using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class TransactionDetailGenericRepository : GenericRepository<TransactionDetail>, ITransactionDetailGenericRepository
{
    public TransactionDetailGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
