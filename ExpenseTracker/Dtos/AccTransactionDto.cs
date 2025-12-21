namespace ExpenseTracker.Dtos;

public class AccTransactionDto
{
    public DateTime TxnDate { get; set; }
    public Decimal Amount { get; set; }
    public string Type { get; set; }
    public int TypeId { get; set; }
    public string? Remarks { get; set; }
    public bool IsJv { get; set; }
    public List<TransactionDetailDto> Details { get; set; }
}

public class TransactionDetailDto
{
    public bool IsDr { get; set; }
    public int LedgerID { get; set; }
    public decimal Amount { get; set; }
}

