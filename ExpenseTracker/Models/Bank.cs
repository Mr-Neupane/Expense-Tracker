using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("bank", Schema = "bank")]
    public class Bank
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("bankname")] public string BankName { get; set; }
        [Column("accountnumber")] public string AccountNumber { get; set; }
        [Column("bankcontactnumber")] public long BankContactNumber { get; set; }
        [Column("ledgerid")] public int LedgerId { get; set; }
        [Column("remainingbalance")] public decimal RemainingBalance { get; set; }
        [Column("bankaddress")] public string BankAddress { get; set; }
        [Column("accountopendate")] public DateTime AccountOpendate { get; set; }
        [Column("recstatus")] public char RecStatus { get; set; }
        [Column("recdate")] public DateTime RecDate { get; set; }
        [Column("status")] public int Status { get; set; }
        [Column("recbyid")] public int RecbyId { get; set; }
    }
}