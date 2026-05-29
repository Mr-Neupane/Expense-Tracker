using ExpenseTracker.Data;

namespace ExpenseTracker.Repository;

public class AccountingTransactionRepo : GenericRepository<Models.Transaction>, IAccountingTransactionRepo
{
    public AccountingTransactionRepo(ApplicationDbContext context) : base(context)
    {
    }
}
