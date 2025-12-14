namespace TestApplication.ViewModels;

public class JournalVoucherVm
{
    public string VoucherDate { get; set; }
    public string Narration { get; set; }

    public List<JournalEntryVm> Entries { get; set; } = new List<JournalEntryVm>();
}

public class JournalEntryVm
{
    public int LedgerId { get; set; }
    public string LedgerCode { get; set; }
    public decimal DrAmount { get; set; }
    public decimal CrAmount { get; set; }
}
