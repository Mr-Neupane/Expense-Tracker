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
    
    
    [HttpGet]
    public async Task<IActionResult> EditLedger(int ledgerId)
    {
        var res = await (from l in _context.Ledgers
            join pl in _context.Ledgers on l.SubParentId equals pl.Id
            join c in _context.CoaLedger on pl.Parentid equals c.Id
            where l.Id == ledgerId && l.Status == 1 && l.SubParentId != -2
            select new EditLedgerVM
            {
                LedgerId = l.Id,
                Code = l.Code,
                LedgerName = l.Ledgername,
                ParentName = c.Name,
                SubParentName = pl.Ledgername
            }).FirstOrDefaultAsync();

        if (res != null)
        {
            return View(res);
        }

        else
        {
            _toastNotification.AddAlertToastMessage("Ledger not found");
            return RedirectToAction("LedgerReport");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditLedger(EditLedgerVM vm)
    {
        var existing = await _context.Ledgers.Where(x => x.Ledgername.Trim() == vm.LedgerName.Trim())
            .FirstOrDefaultAsync();
        if (existing != null)
        {
            _toastNotification.AddErrorToastMessage($"{vm.LedgerName} ledger already exists");
            return View(vm);
        }
        else
        {
            var edit = new EditLedgerDto
            {
                LedgerId = vm.LedgerId,
                LedgerName = vm.LedgerName,
            };
            await _ledgerService.EditLedgerAsync(edit);
            _toastNotification.AddSuccessToastMessage("Ledger edited successfully");
            return RedirectToAction("LedgerReport");
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
        var dto = new LedgerStatementDto
        {
            LedgerId = vm.LedgerId,
            DateFrom = vm.DateFrom,
            DateTo = vm.DateTo,
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
        if (res.Statements.Any())
        {
            return View(res);
        }
        else
        {
            _toastNotification.AddAlertToastMessage("No statements found");
            return View();
        }
    }

    [HttpGet]
    public async Task<RedirectToActionResult> DeactivateLedger(int ledgerId)
    {
        bool res = await _ledgerService.DeactivateLedgerAsync(ledgerId);
        if (res)
        {
            _toastNotification.AddSuccessToastMessage("Ledger deactivated successfully");
        }
        else
        {
            _toastNotification.AddErrorToastMessage("Error deactivating ledger. Ledger may have balance.");
        }

        return RedirectToAction("LedgerReport");
    }

    public async Task<RedirectToActionResult> ActivateLedger(int ledgerId)
    {
        await _ledgerService.ActivateLedgerAsync(ledgerId);
        _toastNotification.AddSuccessToastMessage("Ledger activated successfully");
        return RedirectToAction("LedgerReport");
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