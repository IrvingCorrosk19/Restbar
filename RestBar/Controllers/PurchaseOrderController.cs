using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var purchaseOrders = await _purchaseOrderService.GetAllAsync();
                return View(purchaseOrders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar órdenes de compra: {ex.Message}";
                return View(new List<PurchaseOrder>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                Console.WriteLine("[PurchaseOrderController] Create iniciado");
                
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                Console.WriteLine($"[PurchaseOrderController] Proveedores obtenidos: {suppliers?.Count() ?? 0}");
                
                var products = await _productService.GetAllAsync();
                Console.WriteLine($"[PurchaseOrderController] Productos obtenidos: {products?.Count() ?? 0}");
                
                ViewBag.Suppliers = suppliers;
                ViewBag.Products = products;
                
                Console.WriteLine("[PurchaseOrderController] Create completado exitosamente");
                return View(new PurchaseOrder());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PurchaseOrderController] Error en Create: {ex.Message}");
                TempData["Error"] = $"Error al cargar formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrder purchaseOrder, [FromForm] List<Guid> ProductIds, [FromForm] List<decimal> UnitPrices, [FromForm] List<int> Quantities)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Suppliers = await _supplierService.GetActiveSuppliersAsync();
                    ViewBag.Products = await _productService.GetAllAsync();
                    return View(purchaseOrder);
                }

                // Crear los items de la orden de compra
                if (ProductIds != null && ProductIds.Count > 0)
                {
                    purchaseOrder.Items = new List<PurchaseOrderItem>();
                    
                    for (int i = 0; i < ProductIds.Count; i++)
                    {
                        if (ProductIds[i] != Guid.Empty)
                        {
                            var unitPrice = UnitPrices[i];
                            var quantity = Quantities[i];
                            var subtotal = unitPrice * quantity;
                            var taxRate = 0.16m; // 16% IVA
                            var taxAmount = subtotal * taxRate;
                            var totalAmount = subtotal + taxAmount;

                            var item = new PurchaseOrderItem
                            {
                                Id = Guid.NewGuid(),
                                PurchaseOrderId = purchaseOrder.Id,
                                ProductId = ProductIds[i],
                                UnitPrice = unitPrice,
                                Quantity = quantity,
                                Subtotal = subtotal,
                                TaxRate = taxRate,
                                TaxAmount = taxAmount,
                                TotalAmount = totalAmount,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            purchaseOrder.Items.Add(item);
                        }
                    }
                }

                var createdOrder = await _purchaseOrderService.CreateAsync(purchaseOrder);
                TempData["Success"] = $"Orden de compra {createdOrder.OrderNumber} creada exitosamente";
                return RedirectToAction(nameof(Details), new { id = createdOrder.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear orden de compra: {ex.Message}";
                ViewBag.Suppliers = await _supplierService.GetActiveSuppliersAsync();
                ViewBag.Products = await _productService.GetAllAsync();
                return View(purchaseOrder);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderService.GetByIdAsync(id);
                if (purchaseOrder == null)
                {
                    TempData["Error"] = "Orden de compra no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(purchaseOrder);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar orden de compra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderService.GetByIdAsync(id);
                if (purchaseOrder == null)
                {
                    TempData["Error"] = "Orden de compra no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
                {
                    TempData["Error"] = "Solo se pueden editar órdenes en estado Draft";
                    return RedirectToAction(nameof(Details), new { id });
                }

                ViewBag.Suppliers = await _supplierService.GetActiveSuppliersAsync();
                ViewBag.Products = await _productService.GetAllAsync();
                return View(purchaseOrder);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar orden de compra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PurchaseOrder purchaseOrder)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Suppliers = await _supplierService.GetActiveSuppliersAsync();
                    ViewBag.Products = await _productService.GetAllAsync();
                    return View(purchaseOrder);
                }

                var updatedOrder = await _purchaseOrderService.UpdateAsync(purchaseOrder);
                TempData["Success"] = $"Orden de compra {updatedOrder.OrderNumber} actualizada exitosamente";
                return RedirectToAction(nameof(Details), new { id = updatedOrder.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar orden de compra: {ex.Message}";
                ViewBag.Suppliers = await _supplierService.GetActiveSuppliersAsync();
                ViewBag.Products = await _productService.GetAllAsync();
                return View(purchaseOrder);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var approvedOrder = await _purchaseOrderService.ApproveAsync(id);
                TempData["Success"] = $"Orden de compra {approvedOrder.OrderNumber} aprobada exitosamente";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al aprobar orden de compra: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var cancelledOrder = await _purchaseOrderService.CancelAsync(id);
                TempData["Success"] = $"Orden de compra {cancelledOrder.OrderNumber} cancelada exitosamente";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cancelar orden de compra: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _purchaseOrderService.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Orden de compra eliminada exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar la orden de compra";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar orden de compra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // API Endpoints para AJAX
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders()
        {
            try
            {
                var purchaseOrders = await _purchaseOrderService.GetAllAsync();
                return Json(new { success = true, data = purchaseOrders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByStatus(PurchaseOrderStatus status)
        {
            try
            {
                var purchaseOrders = await _purchaseOrderService.GetByStatusAsync(status);
                return Json(new { success = true, data = purchaseOrders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBySupplier(Guid supplierId)
        {
            try
            {
                var purchaseOrders = await _purchaseOrderService.GetBySupplierAsync(supplierId);
                return Json(new { success = true, data = purchaseOrders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var totalCount = await _purchaseOrderService.GetCountAsync();
                var totalAmount = await _purchaseOrderService.GetTotalAmountAsync();
                var pendingOrders = await _purchaseOrderService.GetByStatusAsync(PurchaseOrderStatus.Pending);
                var approvedOrders = await _purchaseOrderService.GetByStatusAsync(PurchaseOrderStatus.Approved);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalCount,
                        totalAmount,
                        pendingCount = pendingOrders.Count(),
                        approvedCount = approvedOrders.Count()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Receive(Guid id, [FromBody] List<PurchaseOrderItem> receivedItems)
        {
            try
            {
                var receivedOrder = await _purchaseOrderService.ReceiveAsync(id, receivedItems);
                return Json(new { success = true, data = receivedOrder });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 