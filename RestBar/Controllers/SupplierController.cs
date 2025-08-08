using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Policy = "InventoryAccess")]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public SupplierController(ISupplierService supplierService, IProductService productService)
        {
            _supplierService = supplierService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _supplierService.GetAllAsync();
                return View(suppliers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en Index: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    return NotFound();
                }

                var products = await _supplierService.GetProductsBySupplierAsync(id);
                ViewBag.Products = products;

                return View(supplier);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en Details: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ContactPerson,Email,Phone,Fax,Address,City,State,PostalCode,Country,TaxId,AccountNumber,Website,Notes,PaymentTerms,LeadTimeDays,IsActive")] Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _supplierService.CreateAsync(supplier);
                    TempData["SuccessMessage"] = "Proveedor creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en Create: {ex.Message}");
                ModelState.AddModelError("", $"Error al crear proveedor: {ex.Message}");
            }

            return View(supplier);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    return NotFound();
                }

                return View(supplier);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en Edit: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description,ContactPerson,Email,Phone,Fax,Address,City,State,PostalCode,Country,TaxId,AccountNumber,Website,Notes,PaymentTerms,LeadTimeDays,IsActive")] Supplier supplier)
        {
            try
            {
                if (id != supplier.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    await _supplierService.UpdateAsync(supplier);
                    TempData["SuccessMessage"] = "Proveedor actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en Edit: {ex.Message}");
                ModelState.AddModelError("", $"Error al actualizar proveedor: {ex.Message}");
            }

            return View(supplier);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var supplier = await _supplierService.GetByIdAsync(id);
                if (supplier == null)
                {
                    return NotFound();
                }

                return View(supplier);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en Delete: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _supplierService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Proveedor eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en DeleteConfirmed: {ex.Message}");
                TempData["ErrorMessage"] = "Error al eliminar proveedor";
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoints para AJAX
        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                return Json(new { success = true, suppliers = suppliers });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en GetSuppliers: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuppliers(string term)
        {
            try
            {
                var suppliers = await _supplierService.SearchSuppliersAsync(term);
                return Json(new { success = true, suppliers = suppliers });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en SearchSuppliers: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierProducts(Guid supplierId)
        {
            try
            {
                var products = await _supplierService.GetProductsBySupplierAsync(supplierId);
                return Json(new { success = true, products = products });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en GetSupplierProducts: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var supplier = new Supplier
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ContactPerson = dto.ContactPerson,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Fax = dto.Fax,
                    Address = dto.Address,
                    City = dto.City,
                    State = dto.State,
                    PostalCode = dto.PostalCode,
                    Country = dto.Country,
                    TaxId = dto.TaxId,
                    AccountNumber = dto.AccountNumber,
                    Website = dto.Website,
                    Notes = dto.Notes,
                    PaymentTerms = dto.PaymentTerms,
                    LeadTimeDays = dto.LeadTimeDays,
                    IsActive = true
                };

                var createdSupplier = await _supplierService.CreateAsync(supplier);

                return Json(new { 
                    success = true, 
                    message = "Proveedor creado exitosamente",
                    supplier = new
                    {
                        id = createdSupplier.Id,
                        name = createdSupplier.Name,
                        contactPerson = createdSupplier.ContactPerson,
                        email = createdSupplier.Email,
                        phone = createdSupplier.Phone
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en CreateSupplier: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? TaxId { get; set; }
        public string? AccountNumber { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public string? PaymentTerms { get; set; }
        public int? LeadTimeDays { get; set; }
    }
} 