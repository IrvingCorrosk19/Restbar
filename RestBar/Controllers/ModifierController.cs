using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers;

[Authorize(Policy = "ManagerOrAbove")]
public class ModifierController : Controller
{
    private readonly IModifierService _modifiers;

    public ModifierController(IModifierService modifiers) => _modifiers = modifiers;

    public async Task<IActionResult> Index()
    {
        var list = await _modifiers.GetAllAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, decimal extraCost)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "El nombre es requerido";
            return RedirectToAction(nameof(Index));
        }

        Guid? companyId = Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var cid) ? cid : null;
        Guid? branchId = Guid.TryParse(User.FindFirst("BranchId")?.Value, out var bid) ? bid : null;

        await _modifiers.CreateAsync(new Modifier
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description,
            ExtraCost = Math.Max(0, extraCost),
            IsActive = true,
            CompanyId = companyId,
            BranchId = branchId,
            CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name
        });

        TempData["Success"] = "Modificador creado";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var mod = await _modifiers.GetByIdAsync(id);
        if (mod == null)
        {
            TempData["Error"] = "Modificador no encontrado";
            return RedirectToAction(nameof(Index));
        }

        mod.IsActive = !mod.IsActive;
        mod.UpdatedAt = DateTime.UtcNow;
        await _modifiers.UpdateAsync(mod);
        TempData["Success"] = mod.IsActive ? "Modificador activado" : "Modificador desactivado";
        return RedirectToAction(nameof(Index));
    }
}
