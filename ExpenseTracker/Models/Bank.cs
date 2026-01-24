using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("bank", Schema = "bank")]
    public class Bank : BaseModel
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public long BankContactNumber { get; set; }
        public int LedgerId { get; set; }
        public decimal RemainingBalance { get; set; }
        public string BankAddress { get; set; }
        public DateTime AccountOpenDate { get; set; }
        
        
        
    }
}