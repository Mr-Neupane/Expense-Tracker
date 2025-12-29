namespace ExpenseTracker.Dtos;

public class LedgerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public int? SubParentId { get; set; }
    public string Code { get; set; }
}