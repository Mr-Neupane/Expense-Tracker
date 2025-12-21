namespace ExpenseTracker.Dtos;

public class AccountingTransactionReportDto
{
    public int Id { get; set; }
    public string Ledgername { get; set; }
    public string Lcode { get; set; }
    public DateTime TxnDate { get; set; }
    public string VoucherNo { get; set; }
    public string Remarks { get; set; }
    public string Type { get; set; }
    public string Username { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
}