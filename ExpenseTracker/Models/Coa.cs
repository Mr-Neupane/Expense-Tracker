using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("coa", Schema = "accounting")]
    public class Coa
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("recstatus")] public char RecStatus { get; set; }
        [Column("status")] public int Status { get; set; }
        [Column("recbyid")] public int RecbyId { get; set; }
    }
}