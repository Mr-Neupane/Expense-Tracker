namespace ExpenseTracker.Dtos;

public class NewExpenseDto
{
    public int Id { get; set; }
    public int LedgerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TxnDate { get; set; }
}

public class ExpenseReportDto
{
    public int LedgerId { get; set; }
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TxnDate { get; set; }
    public string VoucherNo { get; set; }
    public string Username { get; set; }
    public int Status { get; set; }
}