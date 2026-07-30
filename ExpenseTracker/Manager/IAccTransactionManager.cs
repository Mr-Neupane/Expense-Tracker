using ExpenseTracker.Dtos;

namespace ExpenseTracker.Manager;

public interface IAccTransactionManager
{
    Task RecordBankTransaction(BankTransactionDto dto, AccTransactionDto accTransaction);
    Task RecordExpenseTransaction(NewExpenseDto dto, AccTransactionDto txnDto);
    Task RecordIncomeTransaction(IncomeDto idto, AccTransactionDto dto);
    Task RecordLiabilityTransaction(LiabilityDto liDto, AccTransactionDto txnDto);
}