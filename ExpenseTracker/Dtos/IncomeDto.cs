namespace ExpenseTracker.Dtos;

public class IncomeDto
{
    public int Id { get; set; }
    public int Ledgerid { get; set; }
    public decimal Amount { get; set; }
    public DateTime TxnDate { get; set; }
}

public class IncomeReportDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string VoucherNo { get; set; }
    public int TransactionId { get; set; }
    public string Username { get; set; }
    public int Status { get; set; }
}