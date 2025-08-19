using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RestBar.Services
{
    public class SupplierService : BaseTrackingService, ISupplierService
    {
        public SupplierService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            return await _context.Users
                .Include(u => u.Branch)
                .ThenInclude(b => b.Company)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            try
            {
                Console.WriteLine($"[SupplierService] GetAllAsync iniciado");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }

                // Incluir relaciones
                query = query.Include(s => s.Products).Include(s => s.Company);
                Console.WriteLine($"[SupplierService] Relaciones incluidas");

                // Ordenar
                query = query.OrderBy(s => s.Name);
                Console.WriteLine($"[SupplierService] Ordenamiento aplicado");

                // Ejecutar consulta
                var result = await query.ToListAsync();
                Console.WriteLine($"[SupplierService] ✅ Consulta ejecutada, {result.Count} proveedores encontrados");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en GetAllAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                // En caso de error, devolver lista vacía
                return new List<Supplier>();
            }
        }

        public async Task<Supplier?> GetByIdAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"[SupplierService] GetByIdAsync iniciado - ID: {id}");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de ID
                query = query.Where(s => s.Id == id);
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }

                // Incluir relaciones
                query = query.Include(s => s.Products).Include(s => s.Company);
                Console.WriteLine($"[SupplierService] Relaciones incluidas");

                // Ejecutar consulta
                var result = await query.FirstOrDefaultAsync();
                Console.WriteLine($"[SupplierService] ✅ Consulta ejecutada, proveedor encontrado: {result != null}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en GetByIdAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<Supplier> CreateAsync(Supplier supplier)
        {
            try
            {
                Console.WriteLine($"[SupplierService] CreateAsync iniciado");
                Console.WriteLine($"[SupplierService] Supplier recibido - Name: {supplier?.Name}, Email: {supplier?.Email}, Phone: {supplier?.Phone}");

                if (supplier == null)
                {
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Supplier es null");
                    throw new ArgumentNullException(nameof(supplier), "El proveedor no puede ser null");
                }

                if (string.IsNullOrWhiteSpace(supplier.Name))
                {
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Nombre de proveedor vacío");
                    throw new ArgumentException("El nombre del proveedor es requerido");
                }

                var user = await GetCurrentUserAsync();
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}");

                supplier.Id = Guid.NewGuid();
                supplier.CompanyId = user?.Branch?.CompanyId;
                supplier.IsActive = true;

                Console.WriteLine($"[SupplierService] ID generado: {supplier.Id}");
                Console.WriteLine($"[SupplierService] CompanyId asignado: {supplier.CompanyId}");

                // Obtener información del usuario actual para tracking
                var currentUser = GetCurrentUser();
                Console.WriteLine($"[SupplierService] Usuario actual para tracking: {currentUser}");

                // Configurar tracking manualmente para asegurar que se aplique
                var currentTime = DateTime.UtcNow;
                supplier.CreatedAt = currentTime;
                supplier.UpdatedAt = currentTime;
                supplier.CreatedBy = currentUser;
                supplier.UpdatedBy = currentUser;

                Console.WriteLine($"[SupplierService] Tracking configurado:");
                Console.WriteLine($"[SupplierService]   - CreatedAt: {supplier.CreatedAt}");
                Console.WriteLine($"[SupplierService]   - UpdatedAt: {supplier.UpdatedAt}");
                Console.WriteLine($"[SupplierService]   - CreatedBy: {supplier.CreatedBy}");
                Console.WriteLine($"[SupplierService]   - UpdatedBy: {supplier.UpdatedBy}");

                _context.Suppliers.Add(supplier);
                Console.WriteLine($"[SupplierService] Proveedor agregado al contexto");

                // Usar el método de tracking automático del BaseTrackingService
                Console.WriteLine($"[SupplierService] Guardando cambios con tracking automático...");
                await SaveChangesWithTrackingAsync();
                Console.WriteLine($"[SupplierService] ✅ Proveedor creado exitosamente");

                return supplier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en CreateAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SupplierService] Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            try
            {
                Console.WriteLine($"[SupplierService] UpdateAsync iniciado - ID: {supplier?.Id}");
                Console.WriteLine($"[SupplierService] Supplier recibido - Name: {supplier?.Name}, Email: {supplier?.Email}, Phone: {supplier?.Phone}");

                if (supplier == null)
                {
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Supplier es null");
                    throw new ArgumentNullException(nameof(supplier), "El proveedor no puede ser null");
                }

                if (string.IsNullOrWhiteSpace(supplier.Name))
                {
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Nombre de proveedor vacío");
                    throw new ArgumentException("El nombre del proveedor es requerido");
                }

                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                Console.WriteLine($"[SupplierService] Buscando proveedor existente con ID: {supplier.Id}");
                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de ID
                query = query.Where(s => s.Id == supplier.Id);
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }
                
                var existingSupplier = await query.FirstOrDefaultAsync();
                
                if (existingSupplier == null)
                {
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Proveedor con ID {supplier.Id} no encontrado");
                    throw new ArgumentException("Proveedor no encontrado");
                }

                Console.WriteLine($"[SupplierService] ✅ Proveedor encontrado - Name: {existingSupplier.Name}");

                // Actualizar propiedades
                Console.WriteLine($"[SupplierService] Actualizando propiedades...");
                Console.WriteLine($"[SupplierService]   - Name: {existingSupplier.Name} -> {supplier.Name}");
                Console.WriteLine($"[SupplierService]   - Email: {existingSupplier.Email} -> {supplier.Email}");
                Console.WriteLine($"[SupplierService]   - Phone: {existingSupplier.Phone} -> {supplier.Phone}");

                existingSupplier.Name = supplier.Name;
                existingSupplier.Description = supplier.Description;
                existingSupplier.ContactPerson = supplier.ContactPerson;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.Fax = supplier.Fax;
                existingSupplier.Address = supplier.Address;
                existingSupplier.City = supplier.City;
                existingSupplier.State = supplier.State;
                existingSupplier.PostalCode = supplier.PostalCode;
                existingSupplier.Country = supplier.Country;
                existingSupplier.TaxId = supplier.TaxId;
                existingSupplier.AccountNumber = supplier.AccountNumber;
                existingSupplier.Website = supplier.Website;
                existingSupplier.Notes = supplier.Notes;
                existingSupplier.PaymentTerms = supplier.PaymentTerms;
                existingSupplier.LeadTimeDays = supplier.LeadTimeDays;
                existingSupplier.IsActive = supplier.IsActive;

                Console.WriteLine($"[SupplierService] Guardando cambios con tracking automático...");
                // Usar el método de tracking automático del BaseTrackingService
                await SaveChangesWithTrackingAsync();
                Console.WriteLine($"[SupplierService] ✅ Proveedor actualizado exitosamente");

                return existingSupplier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en UpdateAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SupplierService] Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"[SupplierService] DeleteAsync iniciado - ID: {id}");

                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de ID
                query = query.Where(s => s.Id == id);
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }
                
                var supplier = await query.FirstOrDefaultAsync();
                
                if (supplier == null)
                {
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Proveedor con ID {id} no encontrado");
                    return false;
                }

                Console.WriteLine($"[SupplierService] ✅ Proveedor encontrado - Name: {supplier.Name}");

                // Verificar si tiene productos asociados
                var hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
                if (hasProducts)
                {
                    var productCount = await _context.Products.CountAsync(p => p.SupplierId == id);
                    Console.WriteLine($"[SupplierService] ❌ ERROR: Proveedor tiene {productCount} productos asociados");
                    throw new InvalidOperationException($"No se puede eliminar el proveedor '{supplier.Name}' porque tiene {productCount} producto(s) asociado(s). Primero debe eliminar o reasignar estos productos.");
                }

                Console.WriteLine($"[SupplierService] ✅ No hay productos asociados, procediendo a eliminar...");

                _context.Suppliers.Remove(supplier);
                Console.WriteLine($"[SupplierService] Proveedor removido del contexto");

                // Usar el método de tracking automático del BaseTrackingService
                await SaveChangesWithTrackingAsync();
                Console.WriteLine($"[SupplierService] ✅ Proveedor eliminado exitosamente");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en DeleteAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SupplierService] Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            try
            {
                Console.WriteLine($"[SupplierService] GetActiveSuppliersAsync iniciado");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }

                // Aplicar filtro de activos
                query = query.Where(s => s.IsActive);
                Console.WriteLine($"[SupplierService] Filtro de activos aplicado");

                // Incluir relaciones
                query = query.Include(s => s.Products).Include(s => s.Company);
                Console.WriteLine($"[SupplierService] Relaciones incluidas");

                // Ordenar
                query = query.OrderBy(s => s.Name);
                Console.WriteLine($"[SupplierService] Ordenamiento aplicado");

                // Ejecutar consulta
                var result = await query.ToListAsync();
                Console.WriteLine($"[SupplierService] ✅ Consulta ejecutada, {result.Count} proveedores activos encontrados");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en GetActiveSuppliersAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Supplier>> GetAllActiveSuppliersAsync()
        {
            try
            {
                Console.WriteLine($"[SupplierService] GetAllActiveSuppliersAsync iniciado");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                Console.WriteLine($"[SupplierService] Query inicial creada");
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }
                
                // Aplicar filtro de activos
                query = query.Where(s => s.IsActive);
                Console.WriteLine($"[SupplierService] Filtro de activos aplicado");
                
                // Incluir relaciones
                query = query.Include(s => s.Products);
                Console.WriteLine($"[SupplierService] Relaciones incluidas");
                
                // Ordenar
                query = query.OrderBy(s => s.Name);
                Console.WriteLine($"[SupplierService] Ordenamiento aplicado");

                var result = await query.ToListAsync();
                Console.WriteLine($"[SupplierService] ✅ Consulta ejecutada, {result.Count} proveedores activos encontrados");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en GetAllActiveSuppliersAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                throw new Exception($"Error al obtener proveedores: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            try
            {
                Console.WriteLine($"[SupplierService] SearchSuppliersAsync iniciado - Term: '{searchTerm}'");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    Console.WriteLine($"[SupplierService] Término de búsqueda vacío, usando GetActiveSuppliersAsync");
                    return await GetActiveSuppliersAsync();
                }

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }

                // Aplicar filtro de activos
                query = query.Where(s => s.IsActive);
                
                // Aplicar filtro de búsqueda
                query = query.Where(s => 
                    s.Name.Contains(searchTerm) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(searchTerm)) ||
                    (s.Email != null && s.Email.Contains(searchTerm)) ||
                    (s.Phone != null && s.Phone.Contains(searchTerm)) ||
                    (s.City != null && s.City.Contains(searchTerm))
                );

                // Incluir relaciones
                query = query.Include(s => s.Products).Include(s => s.Company);
                
                // Ordenar
                query = query.OrderBy(s => s.Name);

                var result = await query.ToListAsync();
                Console.WriteLine($"[SupplierService] ✅ Búsqueda completada, {result.Count} proveedores encontrados");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en SearchSuppliersAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsBySupplierAsync(Guid supplierId)
        {
            try
            {
                Console.WriteLine($"[SupplierService] GetProductsBySupplierAsync iniciado - SupplierId: {supplierId}");

                var query = _context.Products.AsQueryable();
                
                // Aplicar filtros específicos
                query = query.Where(p => p.SupplierId == supplierId && p.IsActive == true);
                Console.WriteLine($"[SupplierService] Filtros específicos aplicados: SupplierId={supplierId}, IsActive=true");
                
                // Incluir relaciones
                query = query.Include(p => p.Category);
                Console.WriteLine($"[SupplierService] Relaciones incluidas");
                
                // Ordenar
                query = query.OrderBy(p => p.Name);
                Console.WriteLine($"[SupplierService] Ordenamiento aplicado");

                var result = await query.ToListAsync();
                Console.WriteLine($"[SupplierService] ✅ Productos obtenidos, {result.Count} productos encontrados");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en GetProductsBySupplierAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"[SupplierService] ExistsAsync iniciado - ID: {id}");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de ID
                query = query.Where(s => s.Id == id);
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }

                var result = await query.AnyAsync();
                Console.WriteLine($"[SupplierService] ✅ Verificación completada: {result}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en ExistsAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                Console.WriteLine($"[SupplierService] GetCountAsync iniciado");
                
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;
                Console.WriteLine($"[SupplierService] Usuario obtenido: {user?.Email}, CompanyId: {companyId}");

                var query = _context.Suppliers.AsQueryable();
                
                // Aplicar filtro de compañía si existe
                if (companyId.HasValue)
                {
                    query = query.Where(s => s.CompanyId == companyId);
                    Console.WriteLine($"[SupplierService] Filtro de compañía aplicado: {companyId}");
                }
                else
                {
                    Console.WriteLine($"[SupplierService] ⚠️ No se aplicó filtro de compañía (CompanyId es null)");
                }

                var result = await query.CountAsync();
                Console.WriteLine($"[SupplierService] ✅ Conteo completado: {result} proveedores");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] ❌ ERROR en GetCountAsync: {ex.Message}");
                Console.WriteLine($"[SupplierService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 