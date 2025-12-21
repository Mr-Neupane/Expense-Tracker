using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace ExpenseTracker.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Liability> Liabilities { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<BankTransaction> BankTransaction { get; set; }
    public DbSet<Transaction> AccountingTransaction { get; set; }
    public DbSet<TransactionDetail> TransactionDetails { get; set; }
    public DbSet<Ledger> Ledgers { get; set; }
}