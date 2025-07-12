using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Services;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    [Authorize(Policy = "KitchenAccess")]
    public class StationOrdersController : Controller
    {
        private readonly IKitchenService _kitchenService;

        public StationOrdersController(IKitchenService kitchenService)
        {
            _kitchenService = kitchenService;
        }

        public async Task<IActionResult> Index(string stationName, string stationType, string stationIcon)
        {
            if (string.IsNullOrEmpty(stationType))
            {
                return BadRequest("El tipo de estación es requerido.");
            }

            var orders = await _kitchenService.GetPendingOrdersByStationTypeAsync(stationType);
            
            var displayName = string.IsNullOrEmpty(stationName) ? stationType : stationName;
            ViewData["Title"] = $"Pedidos de {displayName}";
            ViewData["StationName"] = displayName;
            ViewData["StationType"] = stationType;
            ViewData["StationIcon"] = stationIcon;

            return View(orders);
        }

        // POST: StationOrders/MarkAsReady
        [HttpPost]
        public async Task<IActionResult> MarkAsReady([FromBody] MarkAsReadyRequest request)
        {
            try
            {
                Console.WriteLine($"[Backend] MarkAsReady iniciado");
                Console.WriteLine($"[Backend] Request recibido: {System.Text.Json.JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    Console.WriteLine($"[Backend] Error: Request es null");
                    return BadRequest(new { success = false, message = "Request es requerido" });
                }

                Console.WriteLine($"[Backend] orderId recibido: {request.OrderId}");
                Console.WriteLine($"[Backend] orderId es Guid.Empty: {request.OrderId == Guid.Empty}");
                
                if (request.OrderId == Guid.Empty)
                {
                    Console.WriteLine($"[Backend] Error: orderId es Guid.Empty");
                    return BadRequest(new { success = false, message = "ID de orden inválido" });
                }

                // Obtener el tipo de estación desde el request o desde los parámetros de la URL
                var stationType = request.StationType;
                if (string.IsNullOrEmpty(stationType))
                {
                    // Intentar obtener desde los parámetros de la URL
                    stationType = Request.Query["stationType"].ToString();
                }
                
                if (string.IsNullOrEmpty(stationType))
                {
                    Console.WriteLine($"[Backend] Error: No se pudo determinar el tipo de estación");
                    return BadRequest(new { success = false, message = "Tipo de estación no determinado" });
                }

                Console.WriteLine($"[Backend] Tipo de estación: {stationType}");
                Console.WriteLine($"[Backend] Llamando a MarkItemsAsReadyByStationAsync...");
                
                // Usar el nuevo método que marca solo los items de esta estación
                await _kitchenService.MarkItemsAsReadyByStationAsync(request.OrderId, stationType);
                
                Console.WriteLine($"[Backend] Items de la estación marcados como listos exitosamente");
                return Json(new { success = true, message = "Items de la estación marcados como listos" });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[Backend] KeyNotFoundException: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend] Exception: {ex.Message}");
                Console.WriteLine($"[Backend] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Error al marcar los items como listos" });
            }
        }

        // POST: StationOrders/MarkOrderAsReady
        [HttpPost]
        public async Task<IActionResult> MarkOrderAsReady([FromBody] MarkOrderAsReadyRequest request)
        {
            try
            {
                Console.WriteLine($"[Backend] MarkOrderAsReady iniciado");
                Console.WriteLine($"[Backend] Request recibido: {System.Text.Json.JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    Console.WriteLine($"[Backend] Error: Request es null");
                    return BadRequest(new { success = false, message = "Request es requerido" });
                }

                Console.WriteLine($"[Backend] orderId recibido: {request.OrderId}");
                
                if (request.OrderId == Guid.Empty)
                {
                    Console.WriteLine($"[Backend] Error: orderId es Guid.Empty");
                    return BadRequest(new { success = false, message = "ID de orden inválido" });
                }

                Console.WriteLine($"[Backend] Llamando a MarkOrderAsReadyAsync...");
                
                // Marcar toda la orden como lista
                await _kitchenService.MarkOrderAsReadyAsync(request.OrderId);
                
                Console.WriteLine($"[Backend] Orden marcada como lista exitosamente");
                return Json(new { success = true, message = "Orden marcada como lista" });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[Backend] KeyNotFoundException: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend] Exception: {ex.Message}");
                Console.WriteLine($"[Backend] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Error al marcar la orden como lista" });
            }
        }

        // POST: StationOrders/MarkSpecificItemAsReady
        [HttpPost]
        public async Task<IActionResult> MarkSpecificItemAsReady([FromBody] MarkSpecificItemRequest request)
        {
            try
            {
                Console.WriteLine($"[Backend] MarkSpecificItemAsReady iniciado");
                Console.WriteLine($"[Backend] Request recibido: {System.Text.Json.JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    Console.WriteLine($"[Backend] Error: Request es null");
                    return BadRequest(new { success = false, message = "Request es requerido" });
                }

                Console.WriteLine($"[Backend] orderId recibido: {request.OrderId}");
                Console.WriteLine($"[Backend] itemId recibido: {request.ItemId}");
                
                if (request.OrderId == Guid.Empty || request.ItemId == Guid.Empty)
                {
                    Console.WriteLine($"[Backend] Error: orderId o itemId es Guid.Empty");
                    return BadRequest(new { success = false, message = "ID de orden o item inválido" });
                }

                Console.WriteLine($"[Backend] Llamando a MarkSpecificItemAsReadyAsync...");
                
                // Marcar un item específico como listo
                await _kitchenService.MarkSpecificItemAsReadyAsync(request.OrderId, request.ItemId);
                
                Console.WriteLine($"[Backend] Item marcado como listo exitosamente");
                return Json(new { success = true, message = "Item marcado como listo" });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[Backend] KeyNotFoundException: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend] Exception: {ex.Message}");
                Console.WriteLine($"[Backend] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Error al marcar el item como listo" });
            }
        }
    }

    public class MarkAsReadyRequest
    {
        public Guid OrderId { get; set; }
        public string? StationType { get; set; }
    }

    public class MarkOrderAsReadyRequest
    {
        public Guid OrderId { get; set; }
    }

    public class MarkSpecificItemRequest
    {
        public Guid OrderId { get; set; }
        public Guid ItemId { get; set; }
    }
} 