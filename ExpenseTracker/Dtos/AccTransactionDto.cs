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

public class VoucherDetailDto
{
    public string LedgerName { get; set; }
    public string Code { get; set; }
    public string VoucherNo { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
    public string Type { get; set; }
    public string Remarks { get; set; }
    public DateTime TxnDate { get; set; }
    public string UserName { get; set; }
    public bool IsReverseVoucher { get; set; }

    public int Typeid { get; set; }
    public int TransactionId { get; set; }
    public int Status { get; set; }
}