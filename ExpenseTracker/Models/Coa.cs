using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("coa", Schema = "accounting")]
    public class Coa : BaseEntity
    {
        public string Name { get; set; }
    }
}