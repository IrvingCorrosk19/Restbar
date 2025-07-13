using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class UserAssignmentService : IUserAssignmentService
    {
        private readonly RestBarContext _context;

        public UserAssignmentService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserAssignment>> GetAllAsync()
        {
            return await _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Station)
                .Include(ua => ua.Area)
                .Where(ua => ua.IsActive)
                .OrderBy(ua => ua.AssignedAt)
                .ToListAsync();
        }

        public async Task<UserAssignment?> GetByIdAsync(Guid id)
        {
            return await _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Station)
                .Include(ua => ua.Area)
                .FirstOrDefaultAsync(ua => ua.Id == id);
        }

        public async Task<UserAssignment?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Station)
                .Include(ua => ua.Area)
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsActive);
        }

        public async Task<IEnumerable<UserAssignment>> GetByUserRoleAsync(UserRole role)
        {
            return await _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Station)
                .Include(ua => ua.Area)
                .Where(ua => ua.IsActive && ua.User.Role == role)
                .OrderBy(ua => ua.User.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAssignment>> GetByStationIdAsync(Guid stationId)
        {
            return await _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Station)
                .Where(ua => ua.IsActive && ua.StationId == stationId)
                .OrderBy(ua => ua.AssignedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAssignment>> GetByAreaIdAsync(Guid areaId)
        {
            return await _context.UserAssignments
                .Include(ua => ua.User)
                .Include(ua => ua.Area)
                .Where(ua => ua.IsActive && ua.AreaId == areaId)
                .OrderBy(ua => ua.AssignedAt)
                .ToListAsync();
        }

        public async Task<UserAssignment> CreateAsync(UserAssignment assignment)
        {
            // Desactivar asignaciones anteriores del usuario
            var existingAssignment = await GetActiveByUserIdAsync(assignment.UserId);
            if (existingAssignment != null)
            {
                existingAssignment.IsActive = false;
                existingAssignment.UnassignedAt = DateTime.UtcNow; // ✅ Fecha específica de desasignación
                _context.UserAssignments.Update(existingAssignment);
            }

            assignment.Id = Guid.NewGuid();
            assignment.AssignedAt = DateTime.UtcNow; // ✅ Fecha específica de asignación
            assignment.IsActive = true;

            _context.UserAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task UpdateAsync(UserAssignment assignment)
        {
            _context.UserAssignments.Update(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var assignment = await GetByIdAsync(id);
            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.UnassignedAt = DateTime.UtcNow; // ✅ Fecha específica de desasignación
                _context.UserAssignments.Update(assignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UnassignUserAsync(Guid userId)
        {
            var assignment = await GetActiveByUserIdAsync(userId);
            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.UnassignedAt = DateTime.UtcNow; // ✅ Fecha específica de desasignación
                _context.UserAssignments.Update(assignment);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> HasActiveAssignmentAsync(Guid userId)
        {
            return await _context.UserAssignments
                .AnyAsync(ua => ua.UserId == userId && ua.IsActive);
        }

        public async Task<IEnumerable<User>> GetUnassignedUsersByRoleAsync(UserRole role)
        {
            var assignedUserIds = await _context.UserAssignments
                .Where(ua => ua.IsActive)
                .Select(ua => ua.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => u.Role == role && 
                           u.IsActive == true && 
                           !assignedUserIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Station>> GetAvailableStationsAsync()
        {
            return await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Area>> GetAvailableAreasAsync()
        {
            return await _context.Areas
                .Include(a => a.Branch)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Table>> GetAvailableTablesAsync(Guid? areaId = null)
        {
            var query = _context.Tables
                .Include(t => t.Area)
                .Where(t => t.IsActive == true);

            if (areaId.HasValue)
            {
                query = query.Where(t => t.AreaId == areaId);
            }

            return await query
                .OrderBy(t => t.Area!.Name)
                .ThenBy(t => t.TableNumber)
                .ToListAsync();
        }
    }
} 