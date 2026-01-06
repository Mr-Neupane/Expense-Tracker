using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TestApplication.ViewModels;

public class AccountingTxnVm
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string TxnType { get; set; }

    public int Status { get; set; }
    public List<AccountingTransactionReportDto> AccountingTransactions { get; set; }
    public SelectList TransactionsSelectList { get; set; }
}