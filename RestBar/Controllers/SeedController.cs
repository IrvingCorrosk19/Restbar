using Microsoft.AspNetCore.Mvc;
using RestBar.Services;
using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace RestBar.Controllers
{
    [AllowAnonymous]
    public class SeedController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SeedInventory()
        {
            try
            {
                // Capturar la salida de consola
                var originalOut = Console.Out;
                var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);

                // Ejecutar el seeder
                InventorySeeder.SeedInventoryData();

                // Restaurar la salida original
                Console.SetOut(originalOut);
                var output = stringWriter.ToString();

                TempData["SeedResult"] = output;
                TempData["SeedSuccess"] = true;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["SeedResult"] = $"Error: {ex.Message}";
                TempData["SeedSuccess"] = false;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("/seed-data")]
        public IActionResult QuickSeed()
        {
            try
            {
                // Capturar la salida de consola
                var originalOut = Console.Out;
                var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);

                // Ejecutar el seeder
                InventorySeeder.SeedInventoryData();

                // Restaurar la salida original
                Console.SetOut(originalOut);
                var output = stringWriter.ToString();

                return Content($"<html><body><h2>Resultado de la Inserción de Datos</h2><pre>{output}</pre><br><a href='/Inventory'>Ir al Inventario</a></body></html>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<html><body><h2>Error en la Inserción</h2><pre>Error: {ex.Message}</pre><br><a href='/Inventory'>Ir al Inventario</a></body></html>", "text/html");
            }
        }
    }
} 