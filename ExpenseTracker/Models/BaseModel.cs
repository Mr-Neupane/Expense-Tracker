using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

public class BaseModel
{
    [Key] public int Id { get; set; }
    public char RecStatus { get; set; } = 'A';
    public int Status { get; set; } = 1;
    public int RecById { get; set; } = -1;
    public DateTime RecDate { get; set; } = DateTime.Now.ToUniversalTime();
}