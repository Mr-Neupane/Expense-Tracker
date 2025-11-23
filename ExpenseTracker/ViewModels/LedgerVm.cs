namespace TestApplication.ViewModels;

public class LedgerVm : BaseVm
{
    public int Id { get; set; }
    public int SubParentId { get; set; }
    public int ParentId { get; set; }
    public string LedgerName { get; set; }
}