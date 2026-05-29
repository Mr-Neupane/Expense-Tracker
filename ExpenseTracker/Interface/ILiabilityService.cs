using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace ExpenseTracker.Interface;

public interface ILiabilityService
{
    Task<Liability> RecordLiabilityAsync(LiabilityDto dto);
    Task<List<LiabilityReportDto>> GetAllLiabilityReportAsync();
    Task ReverseLiabilityTransactionAsync(int id);
}

