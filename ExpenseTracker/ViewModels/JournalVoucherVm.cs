namespace TestApplication.ViewModels;

public class JournalVoucherVm:BaseVm
{
    public DateTime VoucherDate { get; set; }
    public string Narration { get; set; }
    public string Type { get; set; } = "Journal Voucher";

    public List<JournalEntryVm> Entries { get; set; } = new List<JournalEntryVm>();
}

public class JournalEntryVm
{
    public int LedgerId { get; set; }
    public string LedgerCode { get; set; }
    public decimal LedgerBalance  { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
}