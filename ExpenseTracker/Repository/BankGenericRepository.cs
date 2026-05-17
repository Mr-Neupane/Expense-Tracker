using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class BankGenericRepository : GenericRepository<Bank>, IBankGenericRepository
{
    public BankGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
