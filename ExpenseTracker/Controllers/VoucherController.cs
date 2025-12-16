using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Providers;
using ExpenseTracker.Services;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NToastNotify;
using TestApplication.ViewModels;

namespace ExpenseTracker.Controllers;

public class VoucherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly VoucherService _voucherService;
    private readonly IToastNotification _toastNotification;

    public VoucherController(ApplicationDbContext context, VoucherService voucherService,
        IToastNotification toastNotification)
    {
        _context = context;
        _voucherService = voucherService;
        _toastNotification = toastNotification;
    }

    private static async Task<int> RecordAccountingTransaction(AccountingTxn model)
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var amount = model.DrAmount != 0 ? model.DrAmount : model.CrAmount;
                    var voucherno = await VoucherNumberProvider.GetVoucherNumber();
                    var query =
                        @"INSERT INTO accounting.transactions ( txn_date, voucher_no, amount, type, type_id, remarks, rec_status, rec_date, status,
                                     rec_by_id)
                                     values ( @txndate, @voucherno, @amount, @type, @typeid, @remarks, @recstatus, @recdate, @status,
                                     @recbyid) returning id ";
                    var ins = await conn.QuerySingleAsync<int>(query, new
                    {
                        model.TxnDate, voucherno, amount, model.Type, model.TypeID, model.Remarks, recstatus = 'A',
                        recdate = DateTime.UtcNow,
                        status = 1, recbyid = -1
                    });

                    var detailquery =
                        @"INSERT INTO accounting.transaction_details (transaction_id, ledger_id, dr_amount, cr_amount, dr_cr, rec_status, status,
                                           rec_by_id)
                                           values (@transactionid, @ledgerid, @dramount,@cramount, @drcr, @recstatus, @status,
                                           @recbyid)";

                    await conn.ExecuteAsync(detailquery, new
                    {
                        transactionid = ins,
                        ledgerid = model.FromLedgerID,
                        model.DrAmount,
                        model.CrAmount,
                        drcr = model.DrAmount != 0 ? 'D' : 'C',
                        recstatus = 'A',
                        status = 1,
                        recbyid = -1
                    });


                    var detailquery2 =
                        @"INSERT INTO accounting.transaction_details (transaction_id, ledger_id, dr_amount, cr_amount, dr_cr, rec_status, status,
                                           rec_by_id)
                                           values (@transactionid, @ledgerid, @dramount,@cramount, @drcr, @recstatus, @status,
                                           @recbyid)";
                    await conn.ExecuteAsync(detailquery2, new
                    {
                        transactionid = ins,
                        ledgerid = model.ToLedgerID,
                        dramount = model.CrAmount,
                        cramount = model.DrAmount,
                        drcr = model.DrAmount != 0 ? 'C' : 'D',
                        recstatus = 'A',
                        status = 1,
                        recbyid = -1
                    });
                    await txn.CommitAsync();
                    return ins;
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public static async Task<int> GetInsertedAccountingId(AccountingTxn m)
    {
        int txnid = await RecordAccountingTransaction(m);
        return txnid;
    }

    public async Task<IActionResult> VoucherDetail(int transactionid)
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"
select ledgername,
       txn_date,voucher_no,
       dr_amount,
       cr_amount,
       dr_cr,
       username,t.id,
       t.type,
       t.type_id,
       remarks,
       code
from accounting.transactions t
         join accounting.transaction_details td on t.id = td.transaction_id
         join accounting.ledger l on l.id = ledger_id
         join users u on u.id = t.rec_by_id
where t.status = 1
  and td.status = 1
  and t.id = @transactionid";
        var report = await conn.QueryAsync(query, new { transactionid });
        return View(report.ToList());
    }

    public async Task<IActionResult> AccountingTransaction()
    {
        var conn = DapperConnectionProvider.GetConnection();
        var query = @"select t.id,voucher_no, txn_date, type, username, remarks, amount,status
from accounting.transactions t
         cross join lateral (select id
                             from accounting.transaction_details t2
                             where t.id = t2.transaction_id
                               and t2.rec_status = 'A'
                               and t2.status = 1
                             limit 1)d
         join users u on u.id = t.rec_by_id
where t.status = 1
  and t.rec_status = 'A';";

        var report = await conn.QueryAsync(query);
        var finalreport = report.ToList();
        if (finalreport.Any() || finalreport is not null)
        {
            return View(finalreport);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult AddJv()
    {
        var model = new JournalVoucherVm();
        model.Entries.Add(new JournalEntryVm());
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddJv(JournalVoucherVm vm)
    {
        try
        {
            var txndate = await DateHelper.GetEnglishDate(vm.VoucherDate);
            var vouchernumber = _voucherService.GetNextJvVoucherNo();

            var transaction = new Transaction
            {
                TxnDate = txndate.ToUniversalTime(),
                VoucherNo = vouchernumber,
                Amount = vm.Entries.Sum(e => e.DrAmount),
                Type = vm.Type,
                TypeId = 0,
                Remarks = vm.Narration,
                RecStatus = vm.RecStatus,
                RecDate = DateTime.UtcNow,
                Status = vm.Status,
                RecById = vm.RecById,
                TransactionDetails = vm.Entries.Select(e => new TransactionDetail
                {
                    LedgerId = e.LedgerId,
                    DrAmount = e.DrAmount,
                    CrAmount = e.CrAmount,
                    DrCr = e.DrAmount != 0 ? 'D' : 'C',
                    RecStatus = vm.RecStatus,
                    Status = vm.Status,
                    RecById = vm.RecById
                }).ToList()
            };
            await _context.AccountingTransaction.AddAsync(transaction);
            await _context.SaveChangesAsync();

            foreach (var data in vm.Entries)
            {
                var conn = DapperConnectionProvider.GetConnection();
                int? query = await conn.QueryFirstOrDefaultAsync<int>(
                    "select ledgerId from bank.bank where ledgerid = @ledgerid",
                    new { ledgerid = data.LedgerId });

                var bankledger = query ?? 0;

                var bankid = await BankService.GetBankIdByLedgerId(bankledger);
                var banktrans = vm.Entries.Where(e => e.LedgerId == bankledger)
                    .Select(e => new BankTransaction
                    {
                        BankId = bankid,
                        TxnDate = vm.VoucherDate.ToUniversalTime(),
                        Amount = e.DrAmount == 0 ? e.CrAmount : e.DrAmount,
                        Type = e.DrAmount != 0 ? "Deposit" : "Withdraw",
                        Remarks = vm.Narration,
                        RecDate = DateTime.UtcNow,
                        RecById = vm.RecById,
                        RecStatus = vm.RecStatus,
                        Status = vm.Status,
                        TransactionId = transaction.Id
                    }).ToList();
                await _context.BankTransaction.AddRangeAsync(banktrans);
                await _context.SaveChangesAsync();
            }

            
            _toastNotification.AddSuccessToastMessage("Journal voucher added successfully");
            return RedirectToAction("VoucherDetail", new { transactionid = transaction.Id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _toastNotification.AddErrorToastMessage("Issue creating voucher."+e.Message);
            return View();
        }
    }

    public async Task<IActionResult> ReverseVoucher(int transactionid, int typeid, string type)
    {
        switch (type)
        {
            case "Expense":
                await ReverseService.ReverseExpense(typeid, transactionid);
                break;
            case "Income":
                await ReverseService.ReverseIncome(typeid, transactionid);
                break;
            case "Liability":
                await ReverseService.ReverseRecordedLiability(typeid, transactionid);
                break;
        }

        return RedirectToAction("AccountingTransaction");
    }
}