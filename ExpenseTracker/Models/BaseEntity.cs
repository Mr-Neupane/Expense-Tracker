using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Constants;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Models;

public class BaseEntity : IEntity
{
    [Key] public int Id { get; set; }
    public char RecStatus { get; set; } = RecordStatusConstants.Active;
    public Status Status { get; set; } = Status.Active;
    [ForeignKey(nameof(RecById))]
    public virtual User RecBy { get; set; }
    public int RecById { get; set; } 
    
    public DateTime RecDate { get; set; } = DateTime.Now.ToUniversalTime();
}