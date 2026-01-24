using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    [Table("bank_transactions", Schema = "bank")]
    public class BankTransaction : BaseModel
    {
        public int BankId { get; set; }

        public DateTime? TxnDate { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; }
        public string? Remarks { get; set; }
        public int TransactionId { get; set; }
    }
}