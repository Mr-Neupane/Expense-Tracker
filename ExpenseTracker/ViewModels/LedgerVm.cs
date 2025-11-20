namespace TestApplication.ViewModels;

public class LedgerVm
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string LedgerName { get; set; }
    public string LedgerCode { get; set; }
    public char RecStatus { get; set; } = 'A';
    public int SubParentId { get; set; } = -1;
    public int RecById { get; set; }
    public int Status { get; set; } = 1;
}