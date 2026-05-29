using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class LedgerRepo : GenericRepository<Ledger>, ILedgerRepo
{
    public LedgerRepo(ApplicationDbContext context) : base(context)
    {
    }
}
