namespace ExpenseTracker.Models;

public class AccountingTxn
{
    public DateTime TxnDate { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
    public string Type { get; set; }
    public int TypeID { get; set; }
    public int FromLedgerID { get; set; }
    public int ToLedgerID { get; set; }
   public string Remarks { get; set; }
}