namespace ExpenseTracker.Dtos;

public class LedgerInfoForJvDto
{
    public int LedgerId { get; set; }
    public string LedgerName { get; set; }
    public decimal LedgerBalance { get; set; }
    public string Code { get; set; }
}