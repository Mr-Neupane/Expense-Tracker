using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    [Table("ledger", Schema = "accounting")]
    public class Ledger : BaseModel
    {
        public int? ParentId { get; set; }
        [Required] public string LedgerName { get; set; }
        [Required] public string Code { get; set; }
        public int? SubParentId { get; set; }
    }
}