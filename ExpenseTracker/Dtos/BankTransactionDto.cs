namespace ExpenseTracker.Dtos;

public class BankTransactionDto
{
    public int BankId { get; set; }
    public DateTime TxnDate { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public string Remarks { get; set; }
}