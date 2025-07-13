using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using RestBar.Interfaces;

namespace RestBar.Services;

public class PersonService : IPersonService
{
    private readonly RestBarContext _context;

    public PersonService(RestBarContext context)
    {
        _context = context;
    }

    // 🎯 MÉTODO ESTRATÉGICO: CREAR PERSONA PARA MESA
    public async Task<Person> CreatePersonAsync(string name, Guid orderId, Guid? companyId = null, Guid? branchId = null)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] CreatePersonAsync() - Iniciando creación de persona...");
            Console.WriteLine($"📋 [PersonService] CreatePersonAsync() - Nombre: {name}, OrderId: {orderId}");

            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = name,
                OrderId = orderId,
                CompanyId = companyId,
                BranchId = branchId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ [PersonService] CreatePersonAsync() - Persona creada exitosamente con ID: {person.Id}");
            return person;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] CreatePersonAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] CreatePersonAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: OBTENER PERSONAS DE UNA ORDEN
    public async Task<IEnumerable<Person>> GetPersonsByOrderAsync(Guid orderId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] GetPersonsByOrderAsync() - Iniciando obtención de personas...");
            Console.WriteLine($"📋 [PersonService] GetPersonsByOrderAsync() - OrderId: {orderId}");

            var persons = await _context.Persons
                .Where(p => p.OrderId == orderId && p.IsActive)
                .Include(p => p.AssignedItems)
                .ToListAsync();

            Console.WriteLine($"📊 [PersonService] GetPersonsByOrderAsync() - Total personas encontradas: {persons.Count}");
            return persons;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] GetPersonsByOrderAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] GetPersonsByOrderAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: ASIGNAR ITEM A PERSONA
    public async Task<bool> AssignItemToPersonAsync(Guid itemId, Guid personId, string personName)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] AssignItemToPersonAsync() - Iniciando asignación...");
            Console.WriteLine($"📋 [PersonService] AssignItemToPersonAsync() - ItemId: {itemId}, PersonId: {personId}, PersonName: {personName}");

            var orderItem = await _context.OrderItems.FindAsync(itemId);
            if (orderItem == null)
            {
                Console.WriteLine($"⚠️ [PersonService] AssignItemToPersonAsync() - Item no encontrado: {itemId}");
                return false;
            }

            orderItem.AssignedToPersonId = personId;
            orderItem.AssignedToPersonName = personName;
            orderItem.IsShared = false;
            orderItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ [PersonService] AssignItemToPersonAsync() - Item asignado exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] AssignItemToPersonAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] AssignItemToPersonAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: MARCAR ITEM COMO COMPARTIDO
    public async Task<bool> MarkItemAsSharedAsync(Guid itemId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] MarkItemAsSharedAsync() - Iniciando marcado como compartido...");
            Console.WriteLine($"📋 [PersonService] MarkItemAsSharedAsync() - ItemId: {itemId}");

            var orderItem = await _context.OrderItems.FindAsync(itemId);
            if (orderItem == null)
            {
                Console.WriteLine($"⚠️ [PersonService] MarkItemAsSharedAsync() - Item no encontrado: {itemId}");
                return false;
            }

            orderItem.IsShared = true;
            orderItem.AssignedToPersonId = null;
            orderItem.AssignedToPersonName = null;
            orderItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ [PersonService] MarkItemAsSharedAsync() - Item marcado como compartido exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] MarkItemAsSharedAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] MarkItemAsSharedAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: CALCULAR TOTAL POR PERSONA
    public async Task<decimal> CalculateTotalByPersonAsync(Guid personId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] CalculateTotalByPersonAsync() - Iniciando cálculo...");
            Console.WriteLine($"📋 [PersonService] CalculateTotalByPersonAsync() - PersonId: {personId}");

            var total = await _context.OrderItems
                .Where(oi => oi.AssignedToPersonId == personId && !oi.IsShared)
                .SumAsync(oi => (oi.Quantity * oi.UnitPrice) - oi.Discount);

            Console.WriteLine($"📊 [PersonService] CalculateTotalByPersonAsync() - Total calculado: ${total:F2}");
            return total;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] CalculateTotalByPersonAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] CalculateTotalByPersonAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: OBTENER ITEMS COMPARTIDOS
    public async Task<IEnumerable<OrderItem>> GetSharedItemsAsync(Guid orderId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] GetSharedItemsAsync() - Iniciando obtención de items compartidos...");
            Console.WriteLine($"📋 [PersonService] GetSharedItemsAsync() - OrderId: {orderId}");

            var sharedItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId && oi.IsShared)
                .Include(oi => oi.Product)
                .ToListAsync();

            Console.WriteLine($"📊 [PersonService] GetSharedItemsAsync() - Total items compartidos: {sharedItems.Count}");
            return sharedItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] GetSharedItemsAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] GetSharedItemsAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: ELIMINAR PERSONA
    public async Task<bool> DeletePersonAsync(Guid personId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonService] DeletePersonAsync() - Iniciando eliminación...");
            Console.WriteLine($"📋 [PersonService] DeletePersonAsync() - PersonId: {personId}");

            var person = await _context.Persons.FindAsync(personId);
            if (person == null)
            {
                Console.WriteLine($"⚠️ [PersonService] DeletePersonAsync() - Persona no encontrada: {personId}");
                return false;
            }

            // Marcar como inactiva en lugar de eliminar
            person.IsActive = false;
            person.UpdatedAt = DateTime.UtcNow;

            // Liberar items asignados
            var assignedItems = await _context.OrderItems
                .Where(oi => oi.AssignedToPersonId == personId)
                .ToListAsync();

            foreach (var item in assignedItems)
            {
                item.AssignedToPersonId = null;
                item.AssignedToPersonName = null;
                item.IsShared = true; // Marcar como compartido
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ [PersonService] DeletePersonAsync() - Persona eliminada exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonService] DeletePersonAsync() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonService] DeletePersonAsync() - StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}
