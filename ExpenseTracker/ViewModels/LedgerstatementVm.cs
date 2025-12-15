namespace TestApplication.ViewModels;

public class LedgerstatementVm
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int LedgerId { get; set; }
    public List<LedgerStatement> LedgerStatements { get; set; }
}

public class LedgerStatement
{
    public string LedgerName { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
    public DateTime TxnDate { get; set; }
}