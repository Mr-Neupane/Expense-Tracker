using ExpenseTracker.Dtos;
namespace TestApplication.ViewModels;

public class AccountingTxnVm
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int Status { get; set; }
    public List<AccountingTransactionReportDto> AccountingTransactions { get; set; }
}