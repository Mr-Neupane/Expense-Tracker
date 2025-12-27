namespace ExpenseTracker.Dtos;

public class BankTransactionDto
{
    public int Id { get; set; }
    public int BankId { get; set; }
    public int LedgerId { get; set; }
    public DateTime TxnDate { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public string? Remarks { get; set; }
}