using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("transaction_details", Schema = "accounting")]
    public class TransactionDetail : BaseModel
    {
        public virtual Transaction Transaction { get; set; }
        public int TransactionId { get; set; }
        public int LedgerId { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public char DrCr { get; set; }
    }
}