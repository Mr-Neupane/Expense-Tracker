using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Providers;

public class IProvider
{
    private readonly ILedgerRepo _ledgerGenericRepo;
    private readonly IBankRepo _bankGenericRepo;

    public IProvider(ILedgerRepo ledgerGenericRepo, IBankRepo bankGenericRepo)
    {
        _ledgerGenericRepo = ledgerGenericRepo;
        _bankGenericRepo = bankGenericRepo;
    }

    public async Task<string> GetLedgerCode(int? subparentid)
    {
        var parentLedger = await _ledgerGenericRepo.SingleOrDefaultAsync(x => x.Id == subparentid);
        if (parentLedger == null)
            throw new Exception("Ledger code not found");

        var newCode = await _ledgerGenericRepo.CountAsync(x => x.SubParentId == subparentid) + 1;
        return string.Concat(parentLedger.Code, '.', newCode);
    }

    public async Task<int> GetBankLedgerId(int bankid)
    {
        var bank = await _bankGenericRepo.SingleOrDefaultAsync(x => x.Id == bankid);
        if (bank == null)
            throw new Exception("Bank not found");
        return bank.LedgerId;
    }

    public async Task<bool> ValidateLedgerCode(string ledgercode)
    {
        return await _ledgerGenericRepo.GetBaseQueryable()
            .Select(x => x.Code == ledgercode).FirstOrDefaultAsync();
    }

    public async Task<int?> GetBankIdByLedgerId(int ledgerId)
    {
        return await _bankGenericRepo.GetBaseQueryable()
            .Where(x => x.LedgerId == ledgerId).Select(x => x.Id)
            .SingleOrDefaultAsync();
    }
}
