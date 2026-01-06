namespace ExpenseTracker.Dtos;

public class TransactionReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string Type { get; set; }
    public int Status { get; set; }
    public List<AccountingTransactionReportDto> AccountingTransactionReport { get; set; }
}

public class AccountingTransactionReportDto
{
    public DateTime TxnDate { get; set; }
    public string VoucherNo { get; set; }
    public string Remarks { get; set; }
    public string Type { get; set; }
    public string Username { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public int TransactionId { get; set; }
}