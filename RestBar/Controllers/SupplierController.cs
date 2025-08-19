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
                var suppliers = await _supplierService.GetAllActiveSuppliersAsync();
                return View(suppliers);
            }
            catch (Exception ex)
            {
                // En caso de error, devolver una lista vacía en lugar de la vista Error
                return View(new List<Supplier>());
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
                Console.WriteLine($"[SupplierController] Create iniciado");
                Console.WriteLine($"[SupplierController] Supplier recibido - Name: {supplier?.Name}, Email: {supplier?.Email}, Phone: {supplier?.Phone}");

                if (ModelState.IsValid)
                {
                    Console.WriteLine($"[SupplierController] ✅ ModelState válido, procediendo a crear...");
                    await _supplierService.CreateAsync(supplier);
                    Console.WriteLine($"[SupplierController] ✅ Proveedor creado exitosamente");
                    TempData["SuccessMessage"] = "Proveedor creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine($"[SupplierController] ❌ ERROR: ModelState inválido");
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    Console.WriteLine($"[SupplierController] Errores: {string.Join(", ", errors)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] ❌ ERROR en Create: {ex.Message}");
                Console.WriteLine($"[SupplierController] Stack trace: {ex.StackTrace}");
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
                Console.WriteLine($"[SupplierController] Edit iniciado - ID: {id}");
                Console.WriteLine($"[SupplierController] Supplier recibido - Name: {supplier?.Name}, Email: {supplier?.Email}, Phone: {supplier?.Phone}");

                if (id != supplier.Id)
                {
                    Console.WriteLine($"[SupplierController] ❌ ERROR: ID no coincide - ID recibido: {id}, Supplier.Id: {supplier.Id}");
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    Console.WriteLine($"[SupplierController] ✅ ModelState válido, procediendo a actualizar...");
                    await _supplierService.UpdateAsync(supplier);
                    Console.WriteLine($"[SupplierController] ✅ Proveedor actualizado exitosamente");
                    TempData["SuccessMessage"] = "Proveedor actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine($"[SupplierController] ❌ ERROR: ModelState inválido");
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    Console.WriteLine($"[SupplierController] Errores: {string.Join(", ", errors)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] ❌ ERROR en Edit: {ex.Message}");
                Console.WriteLine($"[SupplierController] Stack trace: {ex.StackTrace}");
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

        [HttpPost]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            try
            {
                Console.WriteLine($"[SupplierController] DeleteSupplier iniciado - ID: {id}");
                
                var result = await _supplierService.DeleteAsync(id);
                
                if (result)
                {
                    return Json(new { success = true, message = "Proveedor eliminado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el proveedor" });
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[SupplierController] Error en DeleteSupplier: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en DeleteSupplier: {ex.Message}");
                return Json(new { success = false, message = "Error al eliminar proveedor" });
            }
        }

        // API endpoints para AJAX
        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                Console.WriteLine($"[SupplierController] GetSuppliers iniciado");
                
                var suppliers = await _supplierService.GetAllActiveSuppliersAsync();
                Console.WriteLine($"[SupplierController] Proveedores obtenidos del servicio: {suppliers.Count()}");
                
                var suppliersData = suppliers.Select(s => new
                {
                    id = s.Id,
                    name = s.Name ?? "",
                    description = s.Description ?? "",
                    contactPerson = s.ContactPerson ?? "",
                    email = s.Email ?? "",
                    phone = s.Phone ?? "",
                    fax = s.Fax ?? "",
                    address = s.Address ?? "",
                    city = s.City ?? "",
                    state = s.State ?? "",
                    postalCode = s.PostalCode ?? "",
                    country = s.Country ?? "",
                    taxId = s.TaxId ?? "",
                    accountNumber = s.AccountNumber ?? "",
                    website = s.Website ?? "",
                    notes = s.Notes ?? "",
                    paymentTerms = s.PaymentTerms ?? "",
                    leadTimeDays = s.LeadTimeDays,
                    isActive = s.IsActive,
                    products = s.Products?.Count ?? 0
                }).ToList();
                
                Console.WriteLine($"[SupplierController] ✅ Datos transformados: {suppliersData.Count} proveedores");
                return Json(new { success = true, suppliers = suppliersData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] ❌ ERROR en GetSuppliers: {ex.Message}");
                Console.WriteLine($"[SupplierController] Stack trace: {ex.StackTrace}");
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

        [HttpPost]
        public async Task<IActionResult> EditSupplier([FromBody] EditSupplierDto dto)
        {
            try
            {
                Console.WriteLine($"[SupplierController] EditSupplier iniciado - ID: {dto.Id}");
                
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var existingSupplier = await _supplierService.GetByIdAsync(dto.Id);
                if (existingSupplier == null)
                {
                    return Json(new { success = false, message = "Proveedor no encontrado" });
                }

                // Actualizar propiedades
                existingSupplier.Name = dto.Name;
                existingSupplier.Description = dto.Description;
                existingSupplier.ContactPerson = dto.ContactPerson;
                existingSupplier.Email = dto.Email;
                existingSupplier.Phone = dto.Phone;
                existingSupplier.Fax = dto.Fax;
                existingSupplier.Address = dto.Address;
                existingSupplier.City = dto.City;
                existingSupplier.State = dto.State;
                existingSupplier.PostalCode = dto.PostalCode;
                existingSupplier.Country = dto.Country;
                existingSupplier.TaxId = dto.TaxId;
                existingSupplier.AccountNumber = dto.AccountNumber;
                existingSupplier.Website = dto.Website;
                existingSupplier.Notes = dto.Notes;
                existingSupplier.PaymentTerms = dto.PaymentTerms;
                existingSupplier.LeadTimeDays = dto.LeadTimeDays;
                existingSupplier.IsActive = dto.IsActive;

                var updatedSupplier = await _supplierService.UpdateAsync(existingSupplier);

                return Json(new { 
                    success = true, 
                    message = "Proveedor actualizado exitosamente",
                    supplier = new
                    {
                        id = updatedSupplier.Id,
                        name = updatedSupplier.Name,
                        contactPerson = updatedSupplier.ContactPerson,
                        email = updatedSupplier.Email,
                        phone = updatedSupplier.Phone
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierController] Error en EditSupplier: {ex.Message}");
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

    public class EditSupplierDto
    {
        public Guid Id { get; set; }
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
        public bool IsActive { get; set; } = true;
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
} 