using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    [Table("ledger", Schema = "accounting")]
    public class Ledger
    {
        [Column("id")]    public int Id { get; set; }
        [Column("parentid")]    public int Parentid { get; set; }
        [Column("ledgername")]  public string Ledgername { get; set; }
        [Column("recstatus")]   public char RecStatus { get; set; }
        [Column("status")]public int Status { get; set; }
        [Column("recbyid")] public int RecById { get; set; }
        [Column("code")]   public string Code { get; set; }
        [Column("subparentid")] public int? SubParentId { get; set; }
    }
}