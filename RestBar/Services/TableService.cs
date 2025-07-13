using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class TableService : BaseTrackingService, ITableService
    {
        public TableService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        // ✅ NUEVO: Método para obtener el contexto (necesario para FixTableStatus)
        public RestBarContext GetContext()
        {
            return _context;
        }

        public async Task<IEnumerable<Table>> GetAllAsync()
        {
            return await _context.Tables
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<Table?> GetByIdAsync(Guid id)
        {
            return await _context.Tables
                .Include(t => t.Area)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Table> CreateAsync(Table table)
        {
            try
            {
                Console.WriteLine("🔍 [TableService] CreateAsync() - Iniciando creación de mesa...");
                Console.WriteLine($"🔍 [TableService] CreateAsync() - Datos recibidos: TableNumber={table.TableNumber}, Capacity={table.Capacity}, AreaId={table.AreaId}, CompanyId={table.CompanyId}, BranchId={table.BranchId}");
                
                // ✅ NUEVO: Generar ID si no existe
                if (table.Id == Guid.Empty)
                {
                    table.Id = Guid.NewGuid();
                    Console.WriteLine($"🔍 [TableService] CreateAsync() - ID generado: {table.Id}");
                }
                
                // Configurar tracking automático antes de crear
                SetCreatedTracking(table);
                
                table.IsActive = true;
                Console.WriteLine($"🔍 [TableService] CreateAsync() - Mesa configurada: CreatedBy={table.CreatedBy}, CreatedAt={table.CreatedAt}, IsActive={table.IsActive}");
                
                _context.Tables.Add(table);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [TableService] CreateAsync() - Mesa creada exitosamente con ID: {table.Id}");
                return table;
            }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [TableService] CreateAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [TableService] CreateAsync() - StackTrace: {ex.StackTrace}");
            
            // ✅ NUEVO: Log de la excepción interna para más detalles
            if (ex.InnerException != null)
            {
                Console.WriteLine($"❌ [TableService] CreateAsync() - InnerException: {ex.InnerException.Message}");
                Console.WriteLine($"🔍 [TableService] CreateAsync() - InnerException StackTrace: {ex.InnerException.StackTrace}");
            }
            
            // ✅ NUEVO: Log de la entidad que está causando el problema
            Console.WriteLine($"🔍 [TableService] CreateAsync() - Entidad que falló:");
            Console.WriteLine($"  - Id: {table.Id}");
            Console.WriteLine($"  - TableNumber: {table.TableNumber}");
            Console.WriteLine($"  - Capacity: {table.Capacity}");
            Console.WriteLine($"  - Status: {table.Status}");
            Console.WriteLine($"  - IsActive: {table.IsActive}");
            Console.WriteLine($"  - AreaId: {table.AreaId}");
            Console.WriteLine($"  - CompanyId: {table.CompanyId}");
            Console.WriteLine($"  - BranchId: {table.BranchId}");
            Console.WriteLine($"  - CreatedAt: {table.CreatedAt}");
            Console.WriteLine($"  - UpdatedAt: {table.UpdatedAt}");
            Console.WriteLine($"  - CreatedBy: {table.CreatedBy}");
            Console.WriteLine($"  - UpdatedBy: {table.UpdatedBy}");
            
            throw;
        }
        }

        public async Task UpdateAsync(Table table)
        {
            try
            {
                Console.WriteLine("🔍 [TableService] UpdateAsync() - Iniciando actualización de mesa...");
                Console.WriteLine($"🔍 [TableService] UpdateAsync() - Mesa ID: {table.Id}, TableNumber: {table.TableNumber}");
                
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Table>()
                    .FirstOrDefault(e => e.Entity.Id == table.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Configurar tracking automático antes de actualizar
                SetUpdatedTracking(table);
                
                Console.WriteLine($"🔍 [TableService] UpdateAsync() - Mesa configurada: UpdatedBy={table.UpdatedBy}, UpdatedAt={table.UpdatedAt}");

                _context.Tables.Update(table);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [TableService] UpdateAsync() - Mesa actualizada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableService] UpdateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableService] UpdateAsync() - StackTrace: {ex.StackTrace}");
                throw new ApplicationException("Error al actualizar la mesa en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table != null)
            {
                _context.Tables.Remove(table);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Table>> GetByAreaIdAsync(Guid areaId)
        {
            return await _context.Tables
                .Where(t => t.AreaId == areaId)
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<IEnumerable<Table>> GetActiveTablesAsync()
        {
            try
            {
                Console.WriteLine("🔍 [TableService] GetActiveTablesAsync() - Iniciando consulta de mesas activas...");
                
                Console.WriteLine("🔍 [TableService] GetActiveTablesAsync() - Ejecutando consulta LINQ...");
                var tables = await _context.Tables
                    .Where(t => t.IsActive == true)
                    .Include(t => t.Area)
                    .ToListAsync();
                
                Console.WriteLine($"📊 [TableService] GetActiveTablesAsync() - Mesas obtenidas de BD: {tables?.Count ?? 0}");
                
                // ✅ NUEVO: Log detallado de cada mesa obtenida
                if (tables != null && tables.Any())
                {
                    Console.WriteLine("🔍 [TableService] GetActiveTablesAsync() - Detalle de mesas desde BD:");
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"  📋 Mesa BD: ID={table.Id}, Number={table.TableNumber}, Status={table.Status}, Area={table.Area?.Name}, Capacity={table.Capacity}, IsActive={table.IsActive}");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ [TableService] GetActiveTablesAsync() - No se encontraron mesas activas en BD");
                }
                
                Console.WriteLine($"✅ [TableService] GetActiveTablesAsync() - Retornando {tables?.Count ?? 0} mesas activas");
                return tables;
            }
            catch (Exception ex)
            {
                // Puedes registrar el error con un logger si tienes uno configurado
                Console.WriteLine($"❌ [TableService] GetActiveTablesAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableService] GetActiveTablesAsync() - StackTrace: {ex.StackTrace}");
                
                // ✅ NUEVO: Log de la excepción interna para más detalles
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ [TableService] GetActiveTablesAsync() - InnerException: {ex.InnerException.Message}");
                    Console.WriteLine($"🔍 [TableService] GetActiveTablesAsync() - InnerException StackTrace: {ex.InnerException.StackTrace}");
                }

                // Devolver una lista vac�a como fallback (opcional)
                Console.WriteLine("⚠️ [TableService] GetActiveTablesAsync() - Retornando lista vacía como fallback");
                return Enumerable.Empty<Table>();
            }
        }


        public async Task<IEnumerable<Table>> GetTablesByStatusAsync(string status)
        {
            return await _context.Tables
                .Where(t => t.Status.ToString() == status && t.IsActive == true)
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<Table?> GetTableWithOrdersAsync(Guid id)
        {
            return await _context.Tables
                .Include(t => t.Area)
                .Include(t => t.Orders)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Table>> GetAvailableTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive == true && t.Status == TableStatus.Disponible)
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTablesForViewBagAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive == true)
                .Select(t => new { t.Id, TableNumber = t.TableNumber })
                .ToListAsync();
        }

        // ✅ NUEVO: Obtener mesas filtradas por CompanyId y BranchId
        public async Task<IEnumerable<Table>> GetTablesByCompanyAndBranchAsync(Guid companyId, Guid branchId)
        {
            try
            {
                Console.WriteLine($"🔍 [TableService] GetTablesByCompanyAndBranchAsync() - Filtrando por CompanyId: {companyId}, BranchId: {branchId}");
                
                // ✅ NUEVO: Verificar que las columnas existan en la tabla
                Console.WriteLine($"🔍 [TableService] GetTablesByCompanyAndBranchAsync() - Verificando estructura de tabla...");
                
                var allTables = await _context.Tables.ToListAsync();
                Console.WriteLine($"🔍 [TableService] GetTablesByCompanyAndBranchAsync() - Total de mesas en BD: {allTables.Count}");
                
                // ✅ NUEVO: Mostrar algunas mesas para debugging
                var sampleTables = allTables.Take(3).ToList();
                foreach (var table in sampleTables)
                {
                    Console.WriteLine($"🔍 [TableService] GetTablesByCompanyAndBranchAsync() - Mesa muestra: ID={table.Id}, CompanyId={table.CompanyId}, BranchId={table.BranchId}, IsActive={table.IsActive}");
                }
                
                var tables = await _context.Tables
                    .Where(t => t.CompanyId == companyId && t.BranchId == branchId && t.IsActive == true)
                    .Include(t => t.Area)
                    .ToListAsync();
                
                Console.WriteLine($"✅ [TableService] GetTablesByCompanyAndBranchAsync() - Mesas encontradas: {tables.Count}");
                
                // ✅ NUEVO: Mostrar detalles de las mesas encontradas
                foreach (var table in tables)
                {
                    Console.WriteLine($"🔍 [TableService] GetTablesByCompanyAndBranchAsync() - Mesa encontrada: ID={table.Id}, TableNumber={table.TableNumber}, Status={table.Status}, Area={table.Area?.Name ?? "Sin área"}");
                }
                
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableService] GetTablesByCompanyAndBranchAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableService] GetTablesByCompanyAndBranchAsync() - StackTrace: {ex.StackTrace}");
                return new List<Table>();
            }
        }

        // ✅ NUEVO: Obtener mesas activas filtradas por CompanyId y BranchId
        public async Task<IEnumerable<Table>> GetActiveTablesByCompanyAndBranchAsync(Guid companyId, Guid branchId)
        {
            try
            {
                Console.WriteLine($"🔍 [TableService] GetActiveTablesByCompanyAndBranchAsync() - Filtrando por CompanyId: {companyId}, BranchId: {branchId}");
                
                var tables = await _context.Tables
                    .Where(t => t.CompanyId == companyId && t.BranchId == branchId && t.IsActive == true)
                    .Include(t => t.Area)
                    .ToListAsync();
                
                Console.WriteLine($"✅ [TableService] GetActiveTablesByCompanyAndBranchAsync() - Mesas activas encontradas: {tables.Count}");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableService] GetActiveTablesByCompanyAndBranchAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableService] GetActiveTablesByCompanyAndBranchAsync() - StackTrace: {ex.StackTrace}");
                return new List<Table>();
            }
        }
    }
} 