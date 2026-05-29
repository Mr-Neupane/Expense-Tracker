using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class BankTransactionGenericRepository : GenericRepository<BankTransaction>, IBankTransactionGenericRepository
{
    public BankTransactionGenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
