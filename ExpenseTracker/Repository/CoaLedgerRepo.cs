using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class CoaLedgerRepo : GenericRepository<Coa>, ICoaLedgerRepo
{
    public CoaLedgerRepo(ApplicationDbContext context) : base(context)
    {
    }
}
