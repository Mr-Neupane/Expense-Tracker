using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;
using ExpenseTracker.Providers;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using TestApplication.Interface;

namespace ExpenseTracker.Controllers;

public class LedgerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILedgerService _ledgerService;
    private readonly IToastNotification _toastNotification;

    public LedgerController(ApplicationDbContext context, IToastNotification toastNotification,
        ILedgerService ledgerService)
    {
        _context = context;
        _toastNotification = toastNotification;
        _ledgerService = ledgerService;
    }

    [HttpGet]
    public async Task<IActionResult> CreateLedger()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateLedger(LedgerVm vm)
    {
        try
        {
            var exists = await (from l in _context.Ledgers where l.Ledgername == vm.LedgerName select l)
                .AnyAsync();
            if (!exists)
            {
                await _ledgerService.AddLedgerAsync(new LedgerDto
                {
                    Name = vm.LedgerName,
                    ParentId = vm.ParentId,
                    SubParentId = vm.SubParentId,
                });
                _toastNotification.AddSuccessToastMessage($"{vm.LedgerName} created successfully");
                return RedirectToAction("LedgerReport");
            }
            else
            {
                _toastNotification.AddErrorToastMessage($"Ledger with name {vm.LedgerName} already exists.");
                return View();
            }
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage("Error creating ledger." + e.Message);
            return View();
        }
    }

    public async Task<IActionResult> LedgerReport()
    {
        var res = await _ledgerService.GetLedgerReportAsync();
        return View(res);
    }


    [HttpGet]
    public IActionResult CreateParentLedger()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateParentLedger(ParentledgerVm vm)
    {
        var validation = await LedgerCode.ValidateLedgerCode(vm.ParentCode);

        try
        {
            if (validation == 1)
            {
                _toastNotification.AddInfoToastMessage("Ledger code already exists");
            }
            else
            {
                await _ledgerService.AddLedgerAsync(new LedgerDto
                {
                    Name = vm.ParentLedgerName,
                    ParentId = vm.ParentId,
                    SubParentId = vm.SubParentId,
                    Code = vm.ParentCode
                });
            }

            return RedirectToAction("CreateParentLedger");
        }
        catch (Exception e)
        {
            _toastNotification.AddErrorToastMessage("Error creating parent ledger." + e.Message);
            return View();
        }
    }


    [HttpGet]
    public async Task<IActionResult> ParentLedgerReport()
    {
        var res = await _ledgerService.GetParentLedgerReportAsync();
        return View(res);
    }

    [HttpGet]
    public IActionResult LedgerStatement()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LedgerStatement(LedgerStatementPageVm vm)
    {
        var fromdate = await DateHelper.GetEnglishDate(vm.DateFrom);
        var todate = await DateHelper.GetEnglishDate(vm.DateTo);
        var dto = new LedgerStatementDto
        {
            LedgerId = vm.LedgerId,
            DateFrom = fromdate,
            DateTo = todate,
        };
        var report = await _ledgerService.GetLedgerStatementsAsync(dto);

        var res = new LedgerStatementPageVm
        {
            DateFrom = vm.DateFrom,
            DateTo = vm.DateTo,
            LedgerId = vm.LedgerId,
            Statements = report.Select(d => new LedgerstatementVm
            {
                ReportLedgerId = d.LedgerId,
                ClosingBalance = d.ClosingBalance,
                OpeningBalance = d.OpeningBalance,
                LedgerStatements = new List<LedgerStatement>()
                {
                    new()
                    {
                        TransactionID = d.TransactionID,
                        LedgerId = d.LedgerId,
                        LedgerName = d.LedgerName,
                        DrAmount = d.DrAmount,
                        VoucherNo = d.VoucherNo,
                        CrAmount = d.CrAmount,
                        TxnDate = d.TxnDate,
                    }
                }
            }).ToList()
        };
        return View(res);
    }

    public IActionResult GetSubParents(int parentId)
    {
        var con = DapperConnectionProvider.GetConnection();

        string sql = @"SELECT Id, LedgerName, code
                           FROM accounting.ledger  
                           WHERE ParentId = @ParentId and id not in (-2)";
        var subParents = con.Query(sql, new { ParentId = parentId }).ToList();

        return Json(subParents);
    }
}