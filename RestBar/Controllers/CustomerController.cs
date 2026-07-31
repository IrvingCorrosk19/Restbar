using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers;

[Authorize(Policy = "ManagerOrAbove")]
public class CustomerController : Controller
{
    private readonly ICustomerService _customers;

    public CustomerController(ICustomerService customers) => _customers = customers;

    public async Task<IActionResult> Index(string? q)
    {
        IEnumerable<Customer> list = string.IsNullOrWhiteSpace(q)
            ? await _customers.GetAllAsync()
            : await _customers.SearchCustomersAsync(q);
        ViewBag.Query = q;
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? fullName, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
        {
            TempData["Error"] = "Indique al menos nombre, correo o teléfono";
            return RedirectToAction(nameof(Index));
        }

        Guid? companyId = Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var cid) ? cid : null;
        Guid? branchId = Guid.TryParse(User.FindFirst("BranchId")?.Value, out var bid) ? bid : null;

        await _customers.CreateAsync(new Customer
        {
            Id = Guid.NewGuid(),
            FullName = fullName?.Trim(),
            Email = email?.Trim(),
            Phone = phone?.Trim(),
            LoyaltyPoints = 0,
            CompanyId = companyId,
            BranchId = branchId,
            CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name
        });

        TempData["Success"] = "Cliente creado";
        return RedirectToAction(nameof(Index));
    }
}
