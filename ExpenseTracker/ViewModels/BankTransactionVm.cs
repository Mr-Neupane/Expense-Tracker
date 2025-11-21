namespace ExpenseTracker.ViewModels;

public class BankTransactionVm
{
  public string BankName { get; set; }
  public DateOnly TxnDate { get; set; }
  public decimal Amount { get; set; }
  public string Remarks { get; set; }
  public string Type { get; set; }
}