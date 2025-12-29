using Dapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dtos;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using TestApplication.ViewModels;
using ExpenseTracker.Providers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        var conn = DapperConnectionProvider.GetConnection();
        var result =
            await conn.QueryAsync(
                @"
select l.ledgername as subparentname, ls.ledgername,ls.code, c.name, l.status, username
from accounting.ledger l
         join accounting.ledger ls on l.id = ls.subparentid
         join accounting.coa c on c.id = l.parentid
         join users u on u.id = l.recbyid;
");
        return View(result);
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
        var conn = DapperConnectionProvider.GetConnection();
        var result =
            @"select l.*,c.name,username from accounting.ledger l join accounting.coa c on c.id = l.parentid join users u on u.id = l.recbyid order by l.parentid";
        var res = await conn.QueryAsync(result);
        return View(res);
    }

    [HttpGet]
    public IActionResult LedgerStatement()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LedgerStatement(LedgerstatementVm vm)
    {
        var dto = new LedgerStatementDto
        {
            LedgerId = vm.LedgerId,
            DateFrom = vm.DateFrom,
            DateTo = vm.DateTo,
        };
        var report = await _ledgerService.GetLedgerStatementsAsync(dto);
        return View(report);
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