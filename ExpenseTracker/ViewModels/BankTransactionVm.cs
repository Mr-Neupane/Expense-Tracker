namespace ExpenseTracker.ViewModels;

public class BankTransactionVm
{
  public int BankId { get; set; }
  public string BankName { get; set; }
  public DateTime TxnDate { get; set; }
  public decimal Amount { get; set; }
  public string Remarks { get; set; }
  public string Type { get; set; }

  public char RecStatus { get; set; } = 'A';
  public int Status { get; set; } = 1;
}