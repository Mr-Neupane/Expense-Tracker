using ExpenseTracker.Dtos;
using ExpenseTracker.Models;

namespace TestApplication.Interface;

public interface ILiabilityService
{
    Task<Liability> RecordLiabilityAsync(LiabilityDto dto);
    Task<List<LiabilityReportDto>> GetAllLiabilityReportAsync();
}

