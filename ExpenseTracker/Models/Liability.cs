using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestApplication.ViewModels;

namespace ExpenseTracker.Models;

[Table("liability", Schema = "accounting")]
public class Liability : BaseModel
{
    public int LedgerId { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
    public DateTime TxnDate { get; set; }
}