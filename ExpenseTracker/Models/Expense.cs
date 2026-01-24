using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestApplication.ViewModels;

namespace ExpenseTracker.Models;

[Table("expenses", Schema = "accounting")]
public class Expense : BaseModel
{
    public virtual Ledger Ledger { get; set; }
    public int LedgerId { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
    public DateTime TxnDate { get; set; }
    public DateTime RecDate { get; set; } = DateTime.UtcNow;
}