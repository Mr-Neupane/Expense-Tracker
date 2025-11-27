using TestApplication.ViewModels;

namespace ExpenseTracker.ViewModels;

public class BankTransactionVm : BaseVm
{
    public int BankId { get; set; }
    public DateTime TxnDate { get; set; }
    public decimal Amount { get; set; }
    public string Remarks { get; set; }
    public string Type { get; set; }
}