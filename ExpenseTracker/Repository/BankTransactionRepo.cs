using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public class BankTransactionRepo : GenericRepository<BankTransaction>, IBankTransactionRepo
{
    public BankTransactionRepo(ApplicationDbContext context) : base(context)
    {
    }
}
