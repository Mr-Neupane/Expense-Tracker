using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class BankRepo : GenericRepository<Bank>, IBankRepo
{
    public BankRepo(ApplicationDbContext context) : base(context)
    {
    }
}
