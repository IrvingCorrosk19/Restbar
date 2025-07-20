using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Services
{
    public class InventorySeeder
    {
        public static void SeedInventoryData()
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestBarContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RestBar;Username=postgres;Password=Panama2020$");

            using (var context = new RestBarContext(optionsBuilder.Options))
            {
                try
                {
                    Console.WriteLine("=== Iniciando inserción de datos de inventario ===");

                    // 1. Insertar categorías
                    Console.WriteLine("Insertando categorías...");
                    var categorias = new[]
                    {
                        new Category { Id = Guid.NewGuid(), Name = "Bebidas", Description = "Bebidas y refrescos", IsActive = true },
                        new Category { Id = Guid.NewGuid(), Name = "Comidas", Description = "Platos principales", IsActive = true },
                        new Category { Id = Guid.NewGuid(), Name = "Postres", Description = "Dulces y postres", IsActive = true },
                        new Category { Id = Guid.NewGuid(), Name = "Entradas", Description = "Aperitivos y entradas", IsActive = true }
                    };

                    foreach (var categoria in categorias)
                    {
                        if (!context.Categories.Any(c => c.Name == categoria.Name))
                        {
                            context.Categories.Add(categoria);
                            Console.WriteLine($"✓ Categoría agregada: {categoria.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"- Categoría ya existe: {categoria.Name}");
                        }
                    }

                    // 2. Insertar sucursales
                    Console.WriteLine("Insertando sucursales...");
                    var sucursales = new[]
                    {
                        new Branch { Id = Guid.NewGuid(), Name = "Sucursal Centro", Address = "Av. Principal 123", Phone = "555-0101", IsActive = true },
                        new Branch { Id = Guid.NewGuid(), Name = "Sucursal Norte", Address = "Calle Norte 456", Phone = "555-0202", IsActive = true }
                    };

                    foreach (var sucursal in sucursales)
                    {
                        if (!context.Branches.Any(b => b.Name == sucursal.Name))
                        {
                            context.Branches.Add(sucursal);
                            Console.WriteLine($"✓ Sucursal agregada: {sucursal.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"- Sucursal ya existe: {sucursal.Name}");
                        }
                    }

                    context.SaveChanges();

                    // 3. Insertar productos
                    Console.WriteLine("Insertando productos...");
                    var categoriaBebidas = context.Categories.FirstOrDefault(c => c.Name == "Bebidas");
                    var categoriaComidas = context.Categories.FirstOrDefault(c => c.Name == "Comidas");
                    var categoriaPostres = context.Categories.FirstOrDefault(c => c.Name == "Postres");

                    var productos = new[]
                    {
                        new Product { Id = Guid.NewGuid(), Name = "Coca Cola", Description = "Refresco de cola 350ml", Price = 2.50m, TaxRate = 16.00m, Unit = "Botella", IsActive = true, CategoryId = categoriaBebidas?.Id },
                        new Product { Id = Guid.NewGuid(), Name = "Hamburguesa Clásica", Description = "Hamburguesa con carne, lechuga, tomate y queso", Price = 8.99m, TaxRate = 16.00m, Unit = "Unidad", IsActive = true, CategoryId = categoriaComidas?.Id },
                        new Product { Id = Guid.NewGuid(), Name = "Pizza Margherita", Description = "Pizza con tomate, mozzarella y albahaca", Price = 12.50m, TaxRate = 16.00m, Unit = "Pizza", IsActive = true, CategoryId = categoriaComidas?.Id },
                        new Product { Id = Guid.NewGuid(), Name = "Tiramisú", Description = "Postre italiano con café y mascarpone", Price = 6.99m, TaxRate = 16.00m, Unit = "Porción", IsActive = true, CategoryId = categoriaPostres?.Id }
                    };

                    foreach (var producto in productos)
                    {
                        if (!context.Products.Any(p => p.Name == producto.Name))
                        {
                            context.Products.Add(producto);
                            Console.WriteLine($"✓ Producto agregado: {producto.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"- Producto ya existe: {producto.Name}");
                        }
                    }

                    context.SaveChanges();

                    // 4. Insertar inventario
                    Console.WriteLine("Insertando inventario...");
                    var sucursalCentro = context.Branches.FirstOrDefault(b => b.Name == "Sucursal Centro");
                    var productosList = context.Products.ToList();

                    var inventarios = new[]
                    {
                        new Inventory { Id = Guid.NewGuid(), BranchId = sucursalCentro?.Id, ProductId = productosList.FirstOrDefault(p => p.Name == "Coca Cola")?.Id, Quantity = 50, Unit = "Botella", MinThreshold = 10, LastUpdated = DateTime.Now },
                        new Inventory { Id = Guid.NewGuid(), BranchId = sucursalCentro?.Id, ProductId = productosList.FirstOrDefault(p => p.Name == "Hamburguesa Clásica")?.Id, Quantity = 25, Unit = "Unidad", MinThreshold = 5, LastUpdated = DateTime.Now },
                        new Inventory { Id = Guid.NewGuid(), BranchId = sucursalCentro?.Id, ProductId = productosList.FirstOrDefault(p => p.Name == "Pizza Margherita")?.Id, Quantity = 15, Unit = "Pizza", MinThreshold = 3, LastUpdated = DateTime.Now },
                        new Inventory { Id = Guid.NewGuid(), BranchId = sucursalCentro?.Id, ProductId = productosList.FirstOrDefault(p => p.Name == "Tiramisú")?.Id, Quantity = 8, Unit = "Porción", MinThreshold = 2, LastUpdated = DateTime.Now }
                    };

                    foreach (var inventario in inventarios)
                    {
                        if (inventario.BranchId.HasValue && inventario.ProductId.HasValue)
                        {
                            var existingInventory = context.Inventories.FirstOrDefault(i => i.BranchId == inventario.BranchId && i.ProductId == inventario.ProductId);
                            if (existingInventory == null)
                            {
                                context.Inventories.Add(inventario);
                                Console.WriteLine($"✓ Inventario agregado para: {productosList.FirstOrDefault(p => p.Id == inventario.ProductId)?.Name}");
                            }
                            else
                            {
                                Console.WriteLine($"- Inventario ya existe para: {productosList.FirstOrDefault(p => p.Id == inventario.ProductId)?.Name}");
                            }
                        }
                    }

                    context.SaveChanges();

                    // 5. Verificar datos insertados
                    Console.WriteLine("Verificando datos insertados...");
                    var totalCategorias = context.Categories.Count();
                    var totalSucursales = context.Branches.Count();
                    var totalProductos = context.Products.Count();
                    var totalInventarios = context.Inventories.Count();

                    Console.WriteLine($"✓ Categorías: {totalCategorias}");
                    Console.WriteLine($"✓ Sucursales: {totalSucursales}");
                    Console.WriteLine($"✓ Productos: {totalProductos}");
                    Console.WriteLine($"✓ Inventarios: {totalInventarios}");

                    Console.WriteLine("=== Inserción completada exitosamente ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error durante la inserción: {ex.Message}");
                    Console.WriteLine($"Detalles: {ex.StackTrace}");
                }
            }
        }
    }
} 