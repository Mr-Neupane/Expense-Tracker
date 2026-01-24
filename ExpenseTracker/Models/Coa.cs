using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models

{
    [Table("coa", Schema = "accounting")]
    public class Coa : BaseModel
    {
        public string Name { get; set; }
    }
}