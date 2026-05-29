using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ExpenseTracker.Models
{
    [Table("users", Schema = "public")]
    public class User : IEntity
    {
        [Column("id")] public int Id { get; set; }
        [Column("username")] public string Username { get; set; }
        [Column("password")] public string Password { get; set; }
    }
}