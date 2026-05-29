<<<<<<< HEAD
﻿using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
=======
﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using ExpenseTracker.Models;
>>>>>>> main
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.PropertyInfo?.GetCustomAttribute<ColumnAttribute>() != null)
                    continue;
                var propertyName = property.Name;
                property.SetColumnName(ToSnakeCase(propertyName));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        var regex = new System.Text.RegularExpressions.Regex(@"([a-z])([A-Z])");
        return regex.Replace(input, "$1_$2").ToLower();
    }
}
