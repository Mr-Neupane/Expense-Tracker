using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    [Table("banktransactions", Schema = "bank")]
    public class BankTransaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bank_id")]
        public int BankId { get; set; }

        [Column("txn_date")]
        public DateTime? TxnDate { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("type")]
        public string Type { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("rec_date")]
        public DateTime RecDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("rec_by_id")]
        public int RecById { get; set; }

        [Column("rec_status")]
        public char RecStatus { get; set; } = 'A';

        [Column("status")]
        public int Status { get; set; } = 1;

        [Required]
        [Column("transaction_id")]
        public int TransactionId { get; set; }
    }
}