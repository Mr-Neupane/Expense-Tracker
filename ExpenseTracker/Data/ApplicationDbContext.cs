using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;


namespace ExpenseTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Liability> Liabilities { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<BankTransaction> BankTransaction { get; set; }
    public DbSet<Transaction> AccountingTransaction { get; set; }
    public DbSet<TransactionDetail> TransactionDetails { get; set; }
    public DbSet<Ledger> Ledgers { get; set; }
    public DbSet<Coa> CoaLedger { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var propertyName = property.Name;
                property.SetColumnName(ToSnakeCase(propertyName)); 
            }
        }
    }
    private string ToSnakeCase(string input)
    {
        var regex = new System.Text.RegularExpressions.Regex(@"([a-z])([A-Z])");
        return regex.Replace(input, "$1_$2").ToLower();
    }
}