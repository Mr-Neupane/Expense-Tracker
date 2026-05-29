using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class AccTxnDetailRepo : GenericRepository<TransactionDetail>, IAccTxnDetailRepo
{
    public AccTxnDetailRepo(ApplicationDbContext context) : base(context)
    {
    }
}
