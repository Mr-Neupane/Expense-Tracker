using System.Transactions;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Transaction = ExpenseTracker.Models.Transaction;

namespace TestApplication.ViewModels.Interface;

public interface IVoucherService
{
    Task<Transaction> RecordTransactionAsync(AccTransactionDto dto);
}