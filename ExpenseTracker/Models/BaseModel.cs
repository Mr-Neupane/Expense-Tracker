using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Constants;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Models;

public class BaseModel : IEntity
{
    [Key] public int Id { get; set; }
    public char RecStatus { get; set; } = RecordStatusConstants.Active;
    public Status Status { get; set; } = Status.Active;
    public int RecById { get; set; } = AppConstants.AdminUserId;
    public virtual AppUser RecBy { get; set; }
    public DateTime RecDate { get; set; } = DateTime.Now.ToUniversalTime();
}