using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TestApplication.ViewModels;

public class ExpenseVm : BaseVm
{
    public int ExpenseLedger { get; set; }
    public DateTime TxnDate { get; set; }
    public int ExpenseFromLedger { get; set; }
    public string Type { get; set; } = "Expense";
    public string Remarks { get; set; }
    public decimal Amount { get; set; }

    public SelectList ExpenseLedgers { get; set; }
    public SelectList CashAndBankLedgers { get; set; }
}