using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.RegisterEntities();
        modelBuilder.Entity<User>(entity =>
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
