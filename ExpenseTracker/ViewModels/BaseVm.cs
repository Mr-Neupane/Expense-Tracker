namespace ExpenseTracker.ViewModels;

public class BaseVm
{
    public DateTime TxnDate { get; set; }= DateTime.Now;
    public string? Remarks { get; set; }
}