using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers;

[Authorize(Policy = "OrderAccess")]
public class PersonController : Controller
{
    private readonly IPersonService _personService;
    private readonly IOrderService _orderService;
    private readonly RestBarContext _context;
    private readonly ILogger<PersonController> _logger;

    public PersonController(
        IPersonService personService,
        IOrderService orderService,
        RestBarContext context,
        ILogger<PersonController> logger)
    {
        _personService = personService;
        _orderService = orderService;
        _context = context;
        _logger = logger;
    }

    // --- HELPERS DE SEGURIDAD ---

    /// <summary>Extrae el BranchId del claim del usuario autenticado.</summary>
    private Guid? GetUserBranchId()
    {
        var claim = User.FindFirst("BranchId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Verifica que la orden pertenezca a la misma branch del usuario.
    /// Retorna false si la orden no existe o pertenece a otra branch.
    /// </summary>
    private async Task<bool> OrderBelongsToUserBranchAsync(Guid orderId)
    {
        var userBranchId = GetUserBranchId();

        // SuperAdmin y admin sin branch asignada tienen acceso global
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role is "superadmin" or "admin" && userBranchId == null)
            return true;

        if (userBranchId == null)
            return false;

        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return false;

        return order.BranchId == userBranchId;
    }

    /// <summary>
    /// Verifica que la persona pertenezca a una orden de la branch del usuario.
    /// </summary>
    private async Task<bool> PersonBelongsToUserBranchAsync(Guid personId)
    {
        var userBranchId = GetUserBranchId();

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role is "superadmin" or "admin" && userBranchId == null)
            return true;

        if (userBranchId == null)
            return false;

        var person = await _context.Persons
            .AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == personId);

        if (person?.Order == null) return false;

        return person.Order.BranchId == userBranchId;
    }

    /// <summary>
    /// Verifica que el OrderItem pertenezca a una orden de la branch del usuario.
    /// </summary>
    private async Task<bool> ItemBelongsToUserBranchAsync(Guid itemId)
    {
        var userBranchId = GetUserBranchId();

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role is "superadmin" or "admin" && userBranchId == null)
            return true;

        if (userBranchId == null)
            return false;

        var item = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == itemId);

        if (item?.Order == null) return false;

        return item.Order.BranchId == userBranchId;
    }

    // --- ENDPOINTS ---

    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonRequest request)
    {
        try
        {
            if (request == null || request.OrderId == Guid.Empty)
                return Json(new { success = false, message = "Datos de solicitud inválidos" });

            // IDOR: validar que la orden pertenece a la branch del usuario
            if (!await OrderBelongsToUserBranchAsync(request.OrderId))
            {
                _logger.LogWarning("[PersonController] CreatePerson: acceso denegado. UserId={UserId}, OrderId={OrderId}",
                    User.FindFirst("UserId")?.Value, request.OrderId);
                return Json(new { success = false, message = "No autorizado para esta orden" });
            }

            var person = await _personService.CreatePersonAsync(request.Name, request.OrderId);

            _logger.LogInformation("[PersonController] Persona creada. PersonId={PersonId}, OrderId={OrderId}",
                person.Id, request.OrderId);

            return Json(new
            {
                success = true,
                data = new { id = person.Id, name = person.Name, orderId = person.OrderId },
                message = "Persona creada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en CreatePerson");
            return Json(new { success = false, message = "Error al crear persona" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPersonsByOrder(Guid orderId)
    {
        try
        {
            if (orderId == Guid.Empty)
                return Json(new { success = false, message = "OrderId inválido" });

            // IDOR: validar branch
            if (!await OrderBelongsToUserBranchAsync(orderId))
            {
                _logger.LogWarning("[PersonController] GetPersonsByOrder: acceso denegado. UserId={UserId}, OrderId={OrderId}",
                    User.FindFirst("UserId")?.Value, orderId);
                return Json(new { success = false, message = "No autorizado para esta orden" });
            }

            var persons = await _personService.GetPersonsByOrderAsync(orderId);

            var personsData = persons.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                notes = p.Notes,
                isActive = p.IsActive,
                total = 0
            }).ToList();

            return Json(new { success = true, data = personsData, message = "Personas obtenidas exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en GetPersonsByOrder");
            return Json(new { success = false, message = "Error al obtener personas" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AssignItemToPerson([FromBody] AssignItemRequest request)
    {
        try
        {
            if (request == null || request.ItemId == Guid.Empty || request.PersonId == Guid.Empty)
                return Json(new { success = false, message = "Datos de solicitud inválidos" });

            // IDOR: validar que el item pertenece a la branch del usuario
            if (!await ItemBelongsToUserBranchAsync(request.ItemId))
            {
                _logger.LogWarning("[PersonController] AssignItemToPerson: acceso denegado. UserId={UserId}, ItemId={ItemId}",
                    User.FindFirst("UserId")?.Value, request.ItemId);
                return Json(new { success = false, message = "No autorizado para este item" });
            }

            // IDOR: validar que la persona pertenece a la branch del usuario
            if (!await PersonBelongsToUserBranchAsync(request.PersonId))
            {
                _logger.LogWarning("[PersonController] AssignItemToPerson: persona no autorizada. UserId={UserId}, PersonId={PersonId}",
                    User.FindFirst("UserId")?.Value, request.PersonId);
                return Json(new { success = false, message = "No autorizado para esta persona" });
            }

            var success = await _personService.AssignItemToPersonAsync(request.ItemId, request.PersonId, request.PersonName);

            if (success)
            {
                _logger.LogInformation("[PersonController] Item asignado. ItemId={ItemId}, PersonId={PersonId}",
                    request.ItemId, request.PersonId);
                return Json(new { success = true, message = "Item asignado exitosamente" });
            }

            return Json(new { success = false, message = "No se pudo asignar el item" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en AssignItemToPerson");
            return Json(new { success = false, message = "Error al asignar item" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkItemAsShared([FromBody] MarkSharedRequest request)
    {
        try
        {
            if (request == null || request.ItemId == Guid.Empty)
                return Json(new { success = false, message = "ItemId inválido" });

            // IDOR: validar que el item pertenece a la branch del usuario
            if (!await ItemBelongsToUserBranchAsync(request.ItemId))
            {
                _logger.LogWarning("[PersonController] MarkItemAsShared: acceso denegado. UserId={UserId}, ItemId={ItemId}",
                    User.FindFirst("UserId")?.Value, request.ItemId);
                return Json(new { success = false, message = "No autorizado para este item" });
            }

            var success = await _personService.MarkItemAsSharedAsync(request.ItemId);

            return Json(success
                ? new { success = true, message = "Item marcado como compartido" }
                : new { success = false, message = "No se pudo marcar el item como compartido" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en MarkItemAsShared");
            return Json(new { success = false, message = "Error al marcar item" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTotalByPerson(Guid personId)
    {
        try
        {
            if (personId == Guid.Empty)
                return Json(new { success = false, message = "PersonId inválido" });

            // IDOR: validar que la persona pertenece a la branch del usuario
            if (!await PersonBelongsToUserBranchAsync(personId))
            {
                _logger.LogWarning("[PersonController] GetTotalByPerson: acceso denegado. UserId={UserId}, PersonId={PersonId}",
                    User.FindFirst("UserId")?.Value, personId);
                return Json(new { success = false, message = "No autorizado para esta persona" });
            }

            var total = await _personService.CalculateTotalByPersonAsync(personId);

            return Json(new { success = true, data = new { total }, message = "Total calculado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en GetTotalByPerson");
            return Json(new { success = false, message = "Error al calcular total" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSharedItems(Guid orderId)
    {
        try
        {
            if (orderId == Guid.Empty)
                return Json(new { success = false, message = "OrderId inválido" });

            // IDOR: validar branch
            if (!await OrderBelongsToUserBranchAsync(orderId))
            {
                _logger.LogWarning("[PersonController] GetSharedItems: acceso denegado. UserId={UserId}, OrderId={OrderId}",
                    User.FindFirst("UserId")?.Value, orderId);
                return Json(new { success = false, message = "No autorizado para esta orden" });
            }

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

            return Json(new { success = true, data = itemsData, message = "Items compartidos obtenidos exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en GetSharedItems");
            return Json(new { success = false, message = "Error al obtener items compartidos" });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePerson(Guid personId)
    {
        try
        {
            if (personId == Guid.Empty)
                return Json(new { success = false, message = "PersonId inválido" });

            // IDOR: validar que la persona pertenece a la branch del usuario
            if (!await PersonBelongsToUserBranchAsync(personId))
            {
                _logger.LogWarning("[PersonController] DeletePerson: acceso denegado. UserId={UserId}, PersonId={PersonId}",
                    User.FindFirst("UserId")?.Value, personId);
                return Json(new { success = false, message = "No autorizado para esta persona" });
            }

            var success = await _personService.DeletePersonAsync(personId);

            if (success)
            {
                _logger.LogInformation("[PersonController] Persona eliminada. PersonId={PersonId}", personId);
                return Json(new { success = true, message = "Persona eliminada exitosamente" });
            }

            return Json(new { success = false, message = "No se pudo eliminar la persona" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PersonController] Error en DeletePerson");
            return Json(new { success = false, message = "Error al eliminar persona" });
        }
    }
}

// DTOs
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
