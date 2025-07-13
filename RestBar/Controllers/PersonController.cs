using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

public class PersonController : Controller
{
    private readonly IPersonService _personService;
    private readonly IOrderService _orderService;

    public PersonController(IPersonService personService, IOrderService orderService)
    {
        _personService = personService;
        _orderService = orderService;
    }

    // 🎯 MÉTODO ESTRATÉGICO: CREAR PERSONA PARA MESA
    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonRequest request)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] CreatePerson() - Iniciando creación de persona...");
            Console.WriteLine($"📋 [PersonController] CreatePerson() - Nombre: {request.Name}, OrderId: {request.OrderId}");

            var person = await _personService.CreatePersonAsync(request.Name, request.OrderId);
            
            Console.WriteLine($"✅ [PersonController] CreatePerson() - Persona creada exitosamente con ID: {person.Id}");
            
            return Json(new { 
                success = true, 
                data = new { 
                    id = person.Id, 
                    name = person.Name,
                    orderId = person.OrderId
                },
                message = "Persona creada exitosamente"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] CreatePerson() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] CreatePerson() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al crear persona: {ex.Message}" });
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: OBTENER PERSONAS DE UNA ORDEN
    [HttpGet]
    public async Task<IActionResult> GetPersonsByOrder(Guid orderId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] GetPersonsByOrder() - Iniciando obtención de personas...");
            Console.WriteLine($"📋 [PersonController] GetPersonsByOrder() - OrderId: {orderId}");

            var persons = await _personService.GetPersonsByOrderAsync(orderId);
            
            var personsData = persons.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                notes = p.Notes,
                isActive = p.IsActive,
                total = 0 // Se calculará en el frontend
            }).ToList();

            Console.WriteLine($"📊 [PersonController] GetPersonsByOrder() - Total personas encontradas: {personsData.Count}");
            
            return Json(new { 
                success = true, 
                data = personsData,
                message = "Personas obtenidas exitosamente"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] GetPersonsByOrder() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] GetPersonsByOrder() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al obtener personas: {ex.Message}" });
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: ASIGNAR ITEM A PERSONA
    [HttpPost]
    public async Task<IActionResult> AssignItemToPerson([FromBody] AssignItemRequest request)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] AssignItemToPerson() - Iniciando asignación...");
            Console.WriteLine($"📋 [PersonController] AssignItemToPerson() - ItemId: {request.ItemId}, PersonId: {request.PersonId}, PersonName: {request.PersonName}");

            var success = await _personService.AssignItemToPersonAsync(request.ItemId, request.PersonId, request.PersonName);
            
            if (success)
            {
                Console.WriteLine($"✅ [PersonController] AssignItemToPerson() - Item asignado exitosamente");
                return Json(new { success = true, message = "Item asignado exitosamente" });
            }
            else
            {
                Console.WriteLine($"⚠️ [PersonController] AssignItemToPerson() - No se pudo asignar el item");
                return Json(new { success = false, message = "No se pudo asignar el item" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] AssignItemToPerson() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] AssignItemToPerson() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al asignar item: {ex.Message}" });
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: MARCAR ITEM COMO COMPARTIDO
    [HttpPost]
    public async Task<IActionResult> MarkItemAsShared([FromBody] MarkSharedRequest request)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] MarkItemAsShared() - Iniciando marcado como compartido...");
            Console.WriteLine($"📋 [PersonController] MarkItemAsShared() - ItemId: {request.ItemId}");

            var success = await _personService.MarkItemAsSharedAsync(request.ItemId);
            
            if (success)
            {
                Console.WriteLine($"✅ [PersonController] MarkItemAsShared() - Item marcado como compartido exitosamente");
                return Json(new { success = true, message = "Item marcado como compartido" });
            }
            else
            {
                Console.WriteLine($"⚠️ [PersonController] MarkItemAsShared() - No se pudo marcar el item como compartido");
                return Json(new { success = false, message = "No se pudo marcar el item como compartido" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] MarkItemAsShared() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] MarkItemAsShared() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al marcar item como compartido: {ex.Message}" });
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: CALCULAR TOTAL POR PERSONA
    [HttpGet]
    public async Task<IActionResult> GetTotalByPerson(Guid personId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] GetTotalByPerson() - Iniciando cálculo...");
            Console.WriteLine($"📋 [PersonController] GetTotalByPerson() - PersonId: {personId}");

            var total = await _personService.CalculateTotalByPersonAsync(personId);
            
            Console.WriteLine($"📊 [PersonController] GetTotalByPerson() - Total calculado: ${total:F2}");
            
            return Json(new { 
                success = true, 
                data = new { total = total },
                message = "Total calculado exitosamente"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] GetTotalByPerson() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] GetTotalByPerson() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al calcular total: {ex.Message}" });
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: OBTENER ITEMS COMPARTIDOS
    [HttpGet]
    public async Task<IActionResult> GetSharedItems(Guid orderId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] GetSharedItems() - Iniciando obtención de items compartidos...");
            Console.WriteLine($"📋 [PersonController] GetSharedItems() - OrderId: {orderId}");

            var sharedItems = await _personService.GetSharedItemsAsync(orderId);
            
            var itemsData = sharedItems.Select(item => new
            {
                id = item.Id,
                productName = item.Product?.Name ?? "Producto desconocido",
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                discount = item.Discount,
                total = (item.Quantity * item.UnitPrice) - item.Discount,
                notes = item.Notes
            }).ToList();

            Console.WriteLine($"📊 [PersonController] GetSharedItems() - Total items compartidos: {itemsData.Count}");
            
            return Json(new { 
                success = true, 
                data = itemsData,
                message = "Items compartidos obtenidos exitosamente"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] GetSharedItems() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] GetSharedItems() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al obtener items compartidos: {ex.Message}" });
        }
    }

    // 🎯 MÉTODO ESTRATÉGICO: ELIMINAR PERSONA
    [HttpDelete]
    public async Task<IActionResult> DeletePerson(Guid personId)
    {
        try
        {
            Console.WriteLine($"🔍 [PersonController] DeletePerson() - Iniciando eliminación...");
            Console.WriteLine($"📋 [PersonController] DeletePerson() - PersonId: {personId}");

            var success = await _personService.DeletePersonAsync(personId);
            
            if (success)
            {
                Console.WriteLine($"✅ [PersonController] DeletePerson() - Persona eliminada exitosamente");
                return Json(new { success = true, message = "Persona eliminada exitosamente" });
            }
            else
            {
                Console.WriteLine($"⚠️ [PersonController] DeletePerson() - No se pudo eliminar la persona");
                return Json(new { success = false, message = "No se pudo eliminar la persona" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [PersonController] DeletePerson() - Error: {ex.Message}");
            Console.WriteLine($"🔍 [PersonController] DeletePerson() - StackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error al eliminar persona: {ex.Message}" });
        }
    }
}

// DTOs para las peticiones
public class CreatePersonRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
}

public class AssignItemRequest
{
    public Guid ItemId { get; set; }
    public Guid PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
}

public class MarkSharedRequest
{
    public Guid ItemId { get; set; }
}
