namespace TestApplication.ViewModels;

public class LedgerVm
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string LedgerName { get; set; }
    public char RecStatus { get; set; }
    public bool Status { get; set; }
}