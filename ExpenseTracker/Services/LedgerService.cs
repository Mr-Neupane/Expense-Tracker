using ExpenseTracker.Constants;
using ExpenseTracker.Dtos;
using ExpenseTracker.Interface;
using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Enums;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Services;

public class LedgerService : ILedgerService
{
    private readonly IUow _uow;
    private readonly ILedgerRepo _ledgerGenericRepo;
    private readonly ICoaLedgerRepo _coaGenericRepo;
    private readonly IAccTxnDetailRepo _txnDetailGenericRepo;
    private readonly IAccountingTransactionRepo _txnRepo;
    private readonly IUserRepo _userGenericRepo;
    private readonly IProvider _provider;
    private readonly IBalanceProvider _balanceProvider;

    public LedgerService(IUow uow, ILedgerRepo ledgerGenericRepo,
        ICoaLedgerRepo coaGenericRepo, IAccTxnDetailRepo txnDetailGenericRepo,
        IAccountingTransactionRepo txnRepo, IUserRepo userGenericRepo,
        IProvider provider, IBalanceProvider balanceProvider)
    {
        _uow = uow;
        _ledgerGenericRepo = ledgerGenericRepo;
        _coaGenericRepo = coaGenericRepo;
        _txnDetailGenericRepo = txnDetailGenericRepo;
        _txnRepo = txnRepo;
        _userGenericRepo = userGenericRepo;
        _provider = provider;
        _balanceProvider = balanceProvider;
    }

    public async Task<Ledger> AddLedgerAsync(LedgerDto dto)
    {
        var ledgerCode = dto.IsParent ? dto.Code : await _provider.GetLedgerCode(dto.SubParentId);
        var ledger = new Ledger
        {
            ParentId = dto.ParentId,
            LedgerName = dto.Name,
            RecStatus = RecordStatusConstants.Active,
            Status = Status.Active,
            RecById = UserConstants.AdminUser,
            Code = ledgerCode,
            SubParentId = dto.SubParentId,
        };
        await _uow.AddAsync(ledger);
        await _uow.SaveChangesAsync();
        return ledger;
    }

    public async Task EditLedgerAsync(EditLedgerDto dto)
    {
        var ledger = await _ledgerGenericRepo.FindOrThrowAsync(dto.LedgerId);
        if (dto.LedgerName != ledger.LedgerName)
        {
            ledger.LedgerName = dto.LedgerName;
        }

        await _uow.SaveChangesAsync();
    }

    public Task<List<ParentLedgerReportDto>> GetParentLedgerReportAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<LedgerReportDto>> GetLedgerReportAsync()
    {
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();
        var cQuery = _coaGenericRepo.GetBaseQueryable();
        var uQuery = _userGenericRepo.GetBaseQueryable();

        var res = await (from l in lQuery
                join pl in lQuery on l.SubParentId equals pl.Id
                join c in cQuery on pl.ParentId equals c.Id
                join u in uQuery on l.RecById equals u.Id
                where l.Status == Status.Active
                select new LedgerReportDto
                {
                    LedgerId = l.Id,
                    SubParentName = string.Concat(pl.LedgerName, " [", pl.Code, "]"),
                    SubParentId = pl.Id,
                    LedgerName = l.LedgerName,
                    Code = l.Code,
                    CoaName = c.Name,
                    Status = (int)l.Status,
                    UserName = u.UserName,
                }
            ).ToListAsync();
        return res;
    }

    public async Task<List<LedgerStatement>> GetLedgerStatementsAsync(LedgerStatementDto vm)
    {
        var report =
            await _balanceProvider.GetLedgerOpeningandCosingBalance(vm.LedgerId, vm.DateFrom, vm.DateTo);

        var tQuery = _txnRepo.GetBaseQueryable();
        var tdQuery = _txnDetailGenericRepo.GetBaseQueryable();
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();

        var data = await (from t in tQuery
            join td in tdQuery.Where(d => d.LedgerId == vm.LedgerId) on t.Id equals td.TransactionId
            join td2 in tdQuery on td.TransactionId equals td2.TransactionId
            join l in lQuery on td2.LedgerId equals l.Id
            where td2.LedgerId != vm.LedgerId && t.Status == Status.Active &&
                  td.Status == Status.Active &&
                  t.TxnDate >= vm.DateFrom.ToUniversalTime() &&
                  t.TxnDate <= vm.DateTo.ToUniversalTime()
            group new { t, td2, td, l } by td.TransactionId
            into g
            select new
            {
                TransactionID = g.Key,
                LedgerId = g.Select(x => x.td2.LedgerId).ToList(),
                VoucherNo = g.Select(x => x.t.VoucherNo).First(),
                LedgerNames = g.Select(x => x.l.LedgerName).ToList(),
                DrAmount = g.Select(x => x.td.DrAmount).First(),
                CrAmount = g.Select(x => x.td.CrAmount).First(),
                TxnDate = g.Select(x => x.t.TxnDate).First(),
            }).ToListAsync();
        var statement = data.Select(d => new LedgerStatement
        {
            TransactionID = d.TransactionID,
            LedgerId = d.LedgerId,
            LedgerName = string.Join(", ",
                d.LedgerNames),
            DrAmount = d.DrAmount,
            VoucherNo = d.VoucherNo,
            CrAmount = d.CrAmount,
            TxnDate = d.TxnDate,
            OpeningBalance = report.OpeningBalance,
            ClosingBalance = report.ClosingBalance,
        }).ToList();
        vm.LedgerStatements = statement;
        return statement;
    }

    public async Task<bool> DeactivateLedgerAsync(int ledgerId)
    {
        var ledger = await _ledgerGenericRepo.FindOrThrowAsync(ledgerId);
        var lQuery = _ledgerGenericRepo.GetBaseQueryable();
        var tdQuery = _txnDetailGenericRepo.GetBaseQueryable();

        var validation = await (from l in lQuery
                join t in tdQuery on l.Id equals t.LedgerId
                where t.LedgerId == ledgerId && t.Status == Status.Active && l.SubParentId != (int)LedgerConstants.BankAccount
                select t)
            .ToListAsync();
        if (validation.Count == 0)
        {
            
            await _uow.SoftDeleteAsync<Ledger>(ledger.Id);
            return true;
        }
        else
        {
            var drAmount = validation.Sum(x => x.DrAmount);
            var crAmount = validation.Sum(x => x.CrAmount);
            if (drAmount == crAmount)
            {
                ledger.Status = Status.Active;
                await _uow.SaveChangesAsync();
                return true;
            }
        }

        return false;
    }

    public async Task<List<int>> DeactivateLedgerAsync(List<int> ledgerIds)
    {
        var failed = new List<int>();
        foreach (var id in ledgerIds)
        {
            var ok = await DeactivateLedgerAsync(id);
            if (!ok)
                failed.Add(id);
        }
        return failed;
    }

    public async Task ActivateLedgerAsync(int ledgerId)
    {
        var ledger = await _ledgerGenericRepo.FindOrThrowAsync(ledgerId);
        ledger.Status = Status.Active;
        await _uow.SaveChangesAsync();
    }
}
