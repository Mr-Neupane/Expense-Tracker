using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public static class EntityRegisterer
{
    public static void RegisterEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Transaction>();
        modelBuilder.Entity<TransactionDetail>();
        modelBuilder.Entity<Expense>();
        modelBuilder.Entity<Income>();
        modelBuilder.Entity<Liability>();
        modelBuilder.Entity<Bank>();
        modelBuilder.Entity<BankTransaction>();
        modelBuilder.Entity<Ledger>();
        modelBuilder.Entity<Coa>();
        modelBuilder.Entity<User>();
    }
}
