using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class LedgerGenericRepository : GenericRepository<Ledger>, ILedgerGenericRepository
{
    public LedgerGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
