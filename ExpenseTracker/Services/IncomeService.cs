using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Services;

public class IncomeService : IIncomeService
{
    private readonly IUow _uow;
    private readonly IIncomeRepo _incomeGenericRepo;

    public IncomeService(IUow uow, IIncomeRepo incomeGenericRepo)
    {
        _uow = uow;
        _incomeGenericRepo = incomeGenericRepo;
    }

    public async Task<Income> RecordIncomeAsync(IncomeDto dto)
    {
        var income = new Income
        {
            LedgerId = dto.Ledgerid,
            DrAmount = 0,
            CrAmount = dto.Amount,
            TxnDate = dto.TxnDate.ToUniversalTime(),
            RecDate = DateTime.Now.ToUniversalTime(),
            Status = Status.Active,
            RecStatus = RecordStatusConstants.Active,
            RecById = dto.User.Id
        };
        await _uow.AddAsync(income);
        await _uow.SaveChangesAsync();
        return income;
    }

    public async Task ReverseIncomeAsync(int id)
    {
        var income = await _incomeGenericRepo.FindOrThrowAsync(id);
        if (income.Status == Status.Active)
        {
            income.Status = Status.Reversed;
            await _uow.SaveChangesAsync();
        }
    }

    
}
