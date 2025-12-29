using TestApplication.ViewModels;

public class LedgerStatementDto
{
    public int LedgerId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<LedgerStatement> LedgerStatements { get; set; }
}