namespace TestApplication.ViewModels;

public class IncomeVm : BaseVm
{
    public int IncomeLedger { get; set; }
    public decimal Amount  { get; set; }
    public string Type  { get; set; }="Income";
    public DateTime TxnDate { get; set; }
    public int IncomeFrom { get; set; }
}