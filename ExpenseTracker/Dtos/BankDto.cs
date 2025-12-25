namespace ExpenseTracker.Dtos;

public class BankDto
{
    public int Id { get; set; }
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public long BankContact { get; set; }
    public string BankAddress { get; set; }
    public DateTime AccountOpenDate { get; set; }
    public int LedgerId { get; set; }
    public decimal RemainingBalance { get; set; }
}