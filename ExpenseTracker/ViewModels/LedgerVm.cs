namespace TestApplication.ViewModels;

public class LedgerVm : BaseVm
{
    public int Id { get; set; }
    public int SubParentId { get; set; }
    public int? ParentId { get; set; }
    public string LedgerName { get; set; }
}

public class EditLedgerVM
{
    public int LedgerId { get; set; }
    public string LedgerName { get; set; }
    public string Code { get; set; }
    public string ParentName { get; set; }
    public string SubParentName { get; set; }
}