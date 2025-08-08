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
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    // Intentar obtener el email del usuario
                    var email = user.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                        return email;

                    // Si no hay email, usar el nombre de usuario
                    var username = user.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username))
                        return username;

                    // Si no hay nombre, usar el ID del usuario
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                        return userId;
                }

                // Si no hay usuario autenticado, usar un valor por defecto
                return "system";
            }
            catch
            {
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
    }
} 