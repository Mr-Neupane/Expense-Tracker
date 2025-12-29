namespace TestApplication.ViewModels;

public class ParentledgerVm : BaseVm
{
    public int ParentId { get; set; }
    public string ParentLedgerName { get; set; }
    public int? SubParentId { get; set; }
    public string ParentCode { get; set; }
}