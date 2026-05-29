using ExpenseTracker.Providers;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[Route("DropdownProvider")]
public class DropdownProviderController : Controller
{
    private readonly DropdownProvider _provider;

    public DropdownProviderController(DropdownProvider provider)
    {
        _provider = provider;
    }

    [HttpGet("GetLedgers")]
    public IActionResult GetLedgers()
    {
        var ledgers = _provider.GetLedgers();
        return Json(ledgers);
    }

    [HttpGet("GetLiabilityLedgers")]
    public IActionResult GetLiabilityLedgers()
    {
        var ledgers = _provider.GetLiabilityLedgers();
        return Json(ledgers.Select(x => new { id = x.Id, ledgername = x.Name }));
    }

    [HttpGet("GetCashBankLedgers")]
    public IActionResult GetCashBankLedgers()
    {
        var ledgers = _provider.GetCashBankLedgers();
        return Json(ledgers.Select(x => new { id = x.Id, ledgername = x.Name }));
    }
}
