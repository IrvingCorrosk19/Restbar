using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    /// <summary>
    /// Servicio base que proporciona funcionalidad de tracking automático para entidades
    /// </summary>
    public abstract class BaseTrackingService
    {
        protected readonly RestBarContext _context;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public BaseTrackingService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Configura el tracking automático para una entidad antes de crear
        /// </summary>
        protected void SetCreatedTracking<T>(T entity) where T : class, ITrackableEntity
        {
            var currentUser = GetCurrentUser();
            var currentTime = DateTime.UtcNow;

            entity.CreatedAt = currentTime;
            entity.UpdatedAt = currentTime;
            entity.CreatedBy = currentUser;
            entity.UpdatedBy = currentUser;
        }

        /// <summary>
        /// Configura el tracking automático para una entidad antes de actualizar
        /// </summary>
        protected void SetUpdatedTracking<T>(T entity) where T : class, ITrackableEntity
        {
            var currentUser = GetCurrentUser();
            var currentTime = DateTime.UtcNow;

            entity.UpdatedAt = currentTime;
            entity.UpdatedBy = currentUser;
        }

        /// <summary>
        /// Obtiene el usuario actual del contexto HTTP
        /// </summary>
        protected string GetCurrentUser()
        {
            try
            {
                Console.WriteLine($"[BaseTrackingService] GetCurrentUser iniciado");
                
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user == null)
                {
                    Console.WriteLine($"[BaseTrackingService] ❌ HttpContext o User es null");
                    return "system";
                }

                Console.WriteLine($"[BaseTrackingService] Usuario autenticado: {user.Identity?.IsAuthenticated}");
                
                if (user?.Identity?.IsAuthenticated == true)
                {
                    // Intentar obtener el email del usuario
                    var email = user.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        Console.WriteLine($"[BaseTrackingService] ✅ Usuario obtenido por email: {email}");
                        return email;
                    }

                    // Si no hay email, usar el nombre de usuario
                    var username = user.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username))
                    {
                        Console.WriteLine($"[BaseTrackingService] ✅ Usuario obtenido por username: {username}");
                        return username;
                    }

                    // Si no hay nombre, usar el ID del usuario
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        Console.WriteLine($"[BaseTrackingService] ✅ Usuario obtenido por userId: {userId}");
                        return userId;
                    }

                    // Intentar obtener el UserId personalizado
                    var customUserId = user.FindFirst("UserId")?.Value;
                    if (!string.IsNullOrEmpty(customUserId))
                    {
                        Console.WriteLine($"[BaseTrackingService] ✅ Usuario obtenido por custom UserId: {customUserId}");
                        return customUserId;
                    }

                    Console.WriteLine($"[BaseTrackingService] ⚠️ Usuario autenticado pero no se pudo obtener información específica");
                    
                    // Listar todos los claims disponibles para debug
                    Console.WriteLine($"[BaseTrackingService] Claims disponibles:");
                    foreach (var claim in user.Claims)
                    {
                        Console.WriteLine($"[BaseTrackingService]   - {claim.Type}: {claim.Value}");
                    }
                }
                else
                {
                    Console.WriteLine($"[BaseTrackingService] ⚠️ Usuario no autenticado");
                }

                // Si no hay usuario autenticado, usar un valor por defecto
                Console.WriteLine($"[BaseTrackingService] 🔄 Usando usuario por defecto: system");
                return "system";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BaseTrackingService] ❌ ERROR en GetCurrentUser: {ex.Message}");
                Console.WriteLine($"[BaseTrackingService] Stack trace: {ex.StackTrace}");
                return "system";
            }
        }

        /// <summary>
        /// Configura el tracking automático para múltiples entidades
        /// </summary>
        protected void SetCreatedTrackingForRange<T>(IEnumerable<T> entities) where T : class, ITrackableEntity
        {
            var currentUser = GetCurrentUser();
            var currentTime = DateTime.UtcNow;

            foreach (var entity in entities)
            {
                entity.CreatedAt = currentTime;
                entity.UpdatedAt = currentTime;
                entity.CreatedBy = currentUser;
                entity.UpdatedBy = currentUser;
            }
        }

        /// <summary>
        /// Configura el tracking automático para múltiples entidades en actualización
        /// </summary>
        protected void SetUpdatedTrackingForRange<T>(IEnumerable<T> entities) where T : class, ITrackableEntity
        {
            var currentUser = GetCurrentUser();
            var currentTime = DateTime.UtcNow;

            foreach (var entity in entities)
            {
                entity.UpdatedAt = currentTime;
                entity.UpdatedBy = currentUser;
            }
        }

        /// <summary>
        /// Verifica si una entidad es nueva (no tiene ID o el ID es el valor por defecto)
        /// </summary>
        protected bool IsNewEntity<T>(T entity) where T : class
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null) return true;

            var idValue = idProperty.GetValue(entity);
            return idValue == null || idValue.Equals(Guid.Empty);
        }

        /// <summary>
        /// Guarda los cambios con tracking automático
        /// </summary>
        protected async Task<int> SaveChangesWithTrackingAsync()
        {
            // Configurar tracking automático para entidades modificadas
            var entries = _context.ChangeTracker.Entries<ITrackableEntity>();
            
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    SetCreatedTracking(entry.Entity);
                }
                else if (entry.State == EntityState.Modified)
                {
                    SetUpdatedTracking(entry.Entity);
                }
            }

            return await _context.SaveChangesAsync();
        }
    }
} 