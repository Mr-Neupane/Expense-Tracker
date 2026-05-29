using TestApplication.ViewModels;

namespace ExpenseTracker.ViewModels;

public class BankVm : BaseVm
{
    public int Id { get; set; }
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public long BankContact { get; set; }
    public string BankAddress { get; set; }
    public DateTime AccountOpenDate { get; set; }
}