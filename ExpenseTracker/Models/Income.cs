using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestApplication.ViewModels;

namespace ExpenseTracker.Models;

[Table("income", Schema = "accounting")]
public class Income
{
    [Column("id", TypeName = ("int"))]
    [Key]
    public int Id { get; set; }

    [Column("ledger_id", TypeName = ("int"))]
    public int LedgerId { get; set; }

    [Column("dr_amount", TypeName = "decimal(18,2)")]
    public decimal DrAmount { get; set; }

    [Column("cr_amount", TypeName = "decimal(18,2)")]
    public decimal CrAmount { get; set; }

    [Column("txn_date")] public DateTime TxnDate { get; set; }

    [Column("rec_date")] public DateTime RecDate { get; set; } = DateTime.UtcNow;

    [Column("status")] public int Status { get; set; } = 1;

    [Column("rec_status")] public char RecStatus { get; set; } = 'A';


    [Column("rec_by_id")] public int RecById { get; set; }
}