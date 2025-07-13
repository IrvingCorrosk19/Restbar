using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using RestBar.Models;
using RestBar.Interfaces;

namespace RestBar.Helpers
{
    /// <summary>
    /// Helper para verificar permisos y roles en vistas
    /// </summary>
    public static class AuthorizationHelper
    {
        /// <summary>
        /// Verifica si el usuario actual tiene el rol especificado
        /// </summary>
        public static bool HasRole(this IHtmlHelper htmlHelper, string role)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user.IsInRole(role);
        }

        /// <summary>
        /// Verifica si el usuario actual tiene alguno de los roles especificados
        /// </summary>
        public static bool HasAnyRole(this IHtmlHelper htmlHelper, params string[] roles)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return roles.Any(role => user.IsInRole(role));
        }

        /// <summary>
        /// Obtiene el rol actual del usuario
        /// </summary>
        public static string GetCurrentUserRole(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user.FindFirst("UserRole")?.Value ?? "";
        }

        /// <summary>
        /// Obtiene el ID del usuario actual
        /// </summary>
        public static string GetCurrentUserId(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user.FindFirst("UserId")?.Value ?? "";
        }

        /// <summary>
        /// Obtiene el nombre del usuario actual
        /// </summary>
        public static string GetCurrentUserName(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            return user.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }

        /// <summary>
        /// Renderiza contenido solo si el usuario tiene el rol especificado
        /// </summary>
        public static IHtmlContent RenderIfHasRole(this IHtmlHelper htmlHelper, string role, Func<object, IHtmlContent> template)
        {
            if (htmlHelper.HasRole(role))
            {
                return template(null);
            }
            return HtmlString.Empty;
        }

        /// <summary>
        /// Renderiza contenido solo si el usuario tiene alguno de los roles especificados
        /// </summary>
        public static IHtmlContent RenderIfHasAnyRole(this IHtmlHelper htmlHelper, string[] roles, Func<object, IHtmlContent> template)
        {
            if (htmlHelper.HasAnyRole(roles))
            {
                return template(null);
            }
            return HtmlString.Empty;
        }

        /// <summary>
        /// Genera las clases CSS apropiadas para el rol del usuario
        /// </summary>
        public static string GetRoleCssClass(this IHtmlHelper htmlHelper, string role)
        {
            return role.ToLower() switch
            {
                "superadmin" => "role-superadmin",
                "admin" => "role-admin",
                "manager" => "role-manager",
                "supervisor" => "role-supervisor",
                "waiter" => "role-waiter",
                "cashier" => "role-cashier",
                "chef" => "role-chef",
                "bartender" => "role-bartender",

                "accountant" => "role-accountant",
                "support" => "role-support",
                _ => "role-default"
            };
        }

        /// <summary>
        /// Obtiene el nombre amigable del rol
        /// </summary>
        public static string GetRoleFriendlyName(this IHtmlHelper htmlHelper, string role)
        {
            return role.ToLower() switch
            {
                "superadmin" => "Super Administrador",
                "admin" => "Administrador",
                "manager" => "Gerente",
                "supervisor" => "Supervisor",
                "waiter" => "Mesero",
                "cashier" => "Cajero",
                "chef" => "Cocinero",
                "bartender" => "Bartender",

                "accountant" => "Contador",
                "support" => "Soporte Técnico",
                _ => role
            };
        }

        /// <summary>
        /// Verifica si el usuario puede acceder a una acción específica
        /// </summary>
        public static bool CanAccessAction(this IHtmlHelper htmlHelper, string action)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            var role = user.FindFirst("UserRole")?.Value;
            
            if (string.IsNullOrEmpty(role))
                return false;

            return role.ToLower() switch
            {
                "superadmin" => true, // SuperAdmin tiene acceso a TODO
                "admin" => action is not "superadmin_only", // Admin puede todo excepto funciones de SuperAdmin
                "manager" => action is "orders" or "kitchen" or "payments" or "tables" or "products" or "users" or "reports",
                "supervisor" => action is "orders" or "kitchen" or "payments" or "tables",
                "waiter" => action is "orders" or "tables" or "customers",
                "cashier" => action is "orders" or "payments" or "customers",
                "chef" => action is "kitchen" or "orders",
                "bartender" => action is "orders" or "kitchen",
                "accountant" => action is "payments" or "reports",
                "support" => action is "orders" or "users",
                _ => false
            };
        }

        /// <summary>
        /// Genera un badge HTML para el rol del usuario
        /// </summary>
        public static IHtmlContent RenderRoleBadge(this IHtmlHelper htmlHelper, string role)
        {
            var friendlyName = htmlHelper.GetRoleFriendlyName(role);
            var cssClass = htmlHelper.GetRoleCssClass(role);
            
            return new HtmlString($"<span class=\"role-badge {cssClass}\">{friendlyName}</span>");
        }

        /// <summary>
        /// Genera JavaScript para verificar permisos en el cliente
        /// </summary>
        public static IHtmlContent RenderPermissionScript(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            var role = user.FindFirst("UserRole")?.Value ?? "";
            var userId = user.FindFirst("UserId")?.Value ?? "";
            var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "";
            
            var script = $@"
                <script>
                    // Información del usuario actual
                    window.currentUser = {{
                        id: '{userId}',
                        name: '{userName}',
                        role: '{role}',
                        isAuthenticated: {user.Identity.IsAuthenticated.ToString().ToLower()}
                    }};
                    
                    // Función para verificar permisos
                    window.hasPermission = function(action) {{
                        if (!window.currentUser.isAuthenticated) return false;
                        
                        const role = window.currentUser.role.toLowerCase();
                        
                        switch(role) {{
                            case 'superadmin': return true;
                            case 'admin': return action !== 'superadmin_only';
                            case 'manager': return ['orders', 'kitchen', 'payments', 'tables', 'products', 'users', 'reports'].includes(action);
                            case 'supervisor': return ['orders', 'kitchen', 'payments', 'tables'].includes(action);
                            case 'waiter': return ['orders', 'tables', 'customers'].includes(action);
                            case 'cashier': return ['orders', 'payments', 'customers'].includes(action);
                            case 'chef': return ['kitchen', 'orders'].includes(action);
                            case 'bartender': return ['orders', 'kitchen'].includes(action);
                            case 'accountant': return ['payments', 'reports'].includes(action);
                            case 'support': return ['orders', 'users'].includes(action);
                            default: return false;
                        }}
                    }};
                    
                    // Función para verificar roles
                    window.hasRole = function(role) {{
                        return window.currentUser.role.toLowerCase() === role.toLowerCase();
                    }};
                    
                    // Función para verificar múltiples roles
                    window.hasAnyRole = function(roles) {{
                        return roles.some(role => window.hasRole(role));
                    }};
                    
                    // Función para mostrar/ocultar elementos basado en permisos
                    window.toggleByPermission = function(action, show = true) {{
                        const elements = document.querySelectorAll(`[data-permission='${{action}}']`);
                        elements.forEach(element => {{
                            if (window.hasPermission(action)) {{
                                element.style.display = show ? '' : 'none';
                            }} else {{
                                element.style.display = show ? 'none' : '';
                            }}
                        }});
                    }};
                    
                    // Función para mostrar/ocultar elementos basado en roles
                    window.toggleByRole = function(role, show = true) {{
                        const elements = document.querySelectorAll(`[data-role='${{role}}']`);
                        elements.forEach(element => {{
                            if (window.hasRole(role)) {{
                                element.style.display = show ? '' : 'none';
                            }} else {{
                                element.style.display = show ? 'none' : '';
                            }}
                        }});
                    }};
                    
                    // Aplicar permisos al cargar la página
                    document.addEventListener('DOMContentLoaded', function() {{
                        // Ocultar elementos sin permisos
                        document.querySelectorAll('[data-permission]').forEach(element => {{
                            const permission = element.getAttribute('data-permission');
                            if (!window.hasPermission(permission)) {{
                                element.style.display = 'none';
                            }}
                        }});
                        
                        // Ocultar elementos sin roles
                        document.querySelectorAll('[data-role]').forEach(element => {{
                            const role = element.getAttribute('data-role');
                            if (!window.hasRole(role)) {{
                                element.style.display = 'none';
                            }}
                        }});
                        
                        // Ocultar elementos que requieren múltiples roles
                        document.querySelectorAll('[data-roles]').forEach(element => {{
                            const roles = element.getAttribute('data-roles').split(',');
                            if (!window.hasAnyRole(roles)) {{
                                element.style.display = 'none';
                            }}
                        }});
                    }});
                </script>";
            
            return new HtmlString(script);
        }

        /// <summary>
        /// Renderiza el menú de navegación basado en permisos
        /// </summary>
        public static IHtmlContent RenderNavMenu(this IHtmlHelper htmlHelper)
        {
            var user = htmlHelper.ViewContext.HttpContext.User;
            var role = user.FindFirst("UserRole")?.Value ?? "";
            
            var menuItems = new List<MenuItem>();
            
            // Menú para SuperAdmin
            if (role.ToLower() == "superadmin")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Super Admin", Icon = "fas fa-crown", Url = "/SuperAdmin/Index" },
                    new MenuItem { Name = "Compañías", Icon = "fas fa-building", Url = "/SuperAdmin/Companies" },
                    new MenuItem { Name = "Sucursales", Icon = "fas fa-store", Url = "/SuperAdmin/Branches" },
                    new MenuItem { Name = "Crear Admin", Icon = "fas fa-user-plus", Url = "/SuperAdmin/CreateAdmin" },
                    new MenuItem { Name = "Dashboard", Icon = "fas fa-home", Url = "/Home/Index" }
                });
            }
            // Menú para Admin
            else if (role.ToLower() == "admin")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Dashboard", Icon = "fas fa-home", Url = "/Home/Index" },
                    new MenuItem { Name = "Usuarios", Icon = "fas fa-users", Url = "/User/UserManagement" },
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" },
                    new MenuItem { Name = "Productos", Icon = "fas fa-box", Url = "/Product/Index" },
                    new MenuItem { Name = "Pagos", Icon = "fas fa-credit-card", Url = "/Payment/Index" },
                    new MenuItem { Name = "Reportes", Icon = "fas fa-chart-bar", Url = "/Reports/Index" }
                });
            }
            // Menú para Manager
            else if (role.ToLower() == "manager")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Dashboard", Icon = "fas fa-home", Url = "/Home/Index" },
                    new MenuItem { Name = "Usuarios", Icon = "fas fa-users", Url = "/User/UserManagement" },
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" },
                    new MenuItem { Name = "Productos", Icon = "fas fa-box", Url = "/Product/Index" },
                    new MenuItem { Name = "Pagos", Icon = "fas fa-credit-card", Url = "/Payment/Index" }
                });
            }
            // Menú para Supervisor
            else if (role.ToLower() == "supervisor")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" },
                    new MenuItem { Name = "Cocina", Icon = "fas fa-utensils", Url = "/StationOrders/Index" },
                    new MenuItem { Name = "Pagos", Icon = "fas fa-credit-card", Url = "/Payment/Index" },
                    new MenuItem { Name = "Mesas", Icon = "fas fa-table", Url = "/Table/Index" }
                });
            }
            // Menú para Waiter
            else if (role.ToLower() == "waiter")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" },
                    new MenuItem { Name = "Mesas", Icon = "fas fa-table", Url = "/Table/Index" }
                });
            }
            // Menú para Cashier
            else if (role.ToLower() == "cashier")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" },
                    new MenuItem { Name = "Pagos", Icon = "fas fa-credit-card", Url = "/Payment/Index" }
                });
            }
            // Menú para Chef/Bartender
            else if (role.ToLower() == "chef" || role.ToLower() == "bartender")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Cocina", Icon = "fas fa-utensils", Url = "/StationOrders/Index" },
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" }
                });
            }

            // Menú para Accountant
            else if (role.ToLower() == "accountant")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Pagos", Icon = "fas fa-credit-card", Url = "/Payment/Index" },
                    new MenuItem { Name = "Reportes", Icon = "fas fa-chart-bar", Url = "/Report/Index" }
                });
            }
            // Menú para Support
            else if (role.ToLower() == "support")
            {
                menuItems.AddRange(new[]
                {
                    new MenuItem { Name = "Órdenes", Icon = "fas fa-shopping-cart", Url = "/Order/Index" },
                    new MenuItem { Name = "Usuarios", Icon = "fas fa-users", Url = "/User/UserManagement" }
                });
            }
            
            // Generar HTML del menú
            var menuHtml = "<ul class=\"nav-menu\">";
            foreach (var item in menuItems)
            {
                menuHtml += $"<li class=\"nav-item\"><a href=\"{item.Url}\" class=\"nav-link\"><i class=\"{item.Icon}\"></i> {item.Name}</a></li>";
            }
            menuHtml += "</ul>";
            
            return new HtmlString(menuHtml);
        }
    }

    /// <summary>
    /// Clase para representar elementos del menú
    /// </summary>
    public class MenuItem
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Url { get; set; } = "";
    }
} 