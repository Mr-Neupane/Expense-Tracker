using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("transaction_details", Schema = "accounting")]
    public class TransactionDetail
    {
        [Key] [Column("id")] public int Id { get; set; }

        [Column("transaction_id")] public int TransactionId { get; set; }

        [Column("ledger_id")] public int LedgerId { get; set; }

        [Column("dr_amount")] public decimal DrAmount { get; set; }

        [Column("cr_amount")] public decimal CrAmount { get; set; }

        [Column("dr_cr")] public char DrCr { get; set; }

        [Column("rec_status")] public char RecStatus { get; set; } = 'A';

        [Column("status")] public int Status { get; set; } = 1;

        [Column("rec_by_id")] public int? RecById { get; set; }
    }
}