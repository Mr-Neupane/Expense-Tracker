using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ExpenseTracker.Models
{
    [Table("transactions",Schema = "accounting")]
    public class Transaction
    {
        
        [Column("id")] 
        public int Id { get; set; }

        [Column("txn_date")]
        public DateTime TxnDate { get; set; }

        [Column("voucher_no")]
        public string VoucherNo { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("type_id")]
        public int TypeId { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("rec_status")]
        public char RecStatus { get; set; } = 'A';

        [Column("rec_date")]
        public DateTime RecDate { get; set; } = DateTime.Now;

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("rec_by_id")]
        public int? RecById { get; set; }
    }
}