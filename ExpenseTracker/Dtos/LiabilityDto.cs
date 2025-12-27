namespace ExpenseTracker.Dtos;

public class LiabilityDto
{
    public int Id { get; set; }
    public int LedgerId { get; set; }
    public int BankId { get; set; }
    public DateTime TxnDate { get; set; }
    public decimal Amount { get; set; }
    public string Remarks { get; set; }
}

public class LiabilityReportDto
{
    public int Id { get; set; }
    public int Ledgerid { get; set; }
    public int Transactionid { get; set; }
    public DateTime TxnDate { get; set; }
    public string Voucherno { get; set; }
    public int Status { get; set; }
    public string Username { get; set; }
    public decimal Amount { get; set; }
    public string Remarks { get; set; }
}