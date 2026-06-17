using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models;

[Table("roles", Schema = "public")]
public class Role : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
}
