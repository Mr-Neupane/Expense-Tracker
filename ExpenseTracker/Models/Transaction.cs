using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Enums;


namespace ExpenseTracker.Models
{
    [Table("transactions", Schema = "accounting")]
    public class Transaction : BaseModel
    {
        public DateTime TxnDate { get; set; }
        public VoucherType VoucherType { get; set; }
        public string VoucherNo { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; }

        public int TypeId { get; set; }

        public string? Remarks { get; set; }
        public bool IsReversed { get; set; } = false;
        public int? ReversedId { get; set; }
        public List<TransactionDetail> TransactionDetails { get; set; }
    }
}