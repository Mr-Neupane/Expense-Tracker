namespace ExpenseTracker.ViewModels;

public class LiabilityVm : BaseVm
{
    public int LiabilityLedger { get; set; }
    public int LiabilityFromLedger { get; set; }
    public decimal Amount { get; set; }
}