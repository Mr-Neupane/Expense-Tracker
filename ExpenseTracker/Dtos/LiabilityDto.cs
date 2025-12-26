namespace ExpenseTracker.Dtos;

public class LiabilityDto
{
    public int Id { get; set; }
    public int LedgerId { get; set; }
    public DateTime TxnDate { get; set; }
    public decimal Amount { get; set; }
    public string Remarks { get; set; }
}