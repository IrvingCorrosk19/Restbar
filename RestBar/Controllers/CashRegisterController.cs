using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "CashAccess")]
public class CashRegisterController : Controller
{
    private readonly ICashRegisterService _registers;
    private readonly FeatureFlags _flags;

    public CashRegisterController(ICashRegisterService registers, IOptions<FeatureFlags> flags)
    {
        _registers = registers;
        _flags = flags.Value;
    }

    public async Task<IActionResult> Index()
    {
        if (!_flags.EnableCashModule)
            return View("ModuleDisabled");

        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var list = await _registers.GetBranchRegistersAsync(branchId);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string code, string name, decimal defaultFloat)
    {
        if (!_flags.EnableCashModule)
            return View("ModuleDisabled");

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);

        await _registers.CreateRegisterAsync(new CashRegister
        {
            CompanyId = companyId,
            BranchId = branchId,
            Code = code,
            Name = name,
            DefaultOpeningFloat = defaultFloat
        });

        return RedirectToAction(nameof(Index));
    }
}
