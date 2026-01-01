namespace ExpenseTracker.Dtos;

public class LedgerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public int? SubParentId { get; set; }
    public string Code { get; set; }
}

public class ParentLedgerReportDto
{
    public string ParentLedgerName { get; set; }
    public int LedgerId { get; set; }
    public int Status { get; set; }
    public string LedgerCode { get; set; }
    public string LedgerName { get; set; }
    public string UserName { get; set; }
}

public class LedgerReportDto
{
    public int LedgerId { get; set; }
    public string SubParentName { get; set; }
    public string LedgerName { get; set; }
    public string Code { get; set; }
    public string CoaName { get; set; }
    public int Status { get; set; }
    public string UserName { get; set; }
}