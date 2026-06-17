using HR_Management_System.Data;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class RoleService : IRoleService
    {
        private readonly HRDbContext _context;

        public RoleService(HRDbContext context)
        {
            _context = context;
        }

        // =====================
        // GET ALL
        // =====================
        public async Task<List<Role>> GetAllRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        // =====================
        // GET BY ID
        // =====================
        public async Task<Role?> GetRoleById(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == id);
        }

        // =====================
        // CREATE
        // =====================
        public async Task<Role> CreateRole(Role role)
        {
            // Prevent duplicate role names
            var existing = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == role.Name);

            if (existing != null)
                throw new Exception($"Role '{role.Name}' already exists");

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        // =====================
        // UPDATE
        // =====================
        public async Task<Role?> UpdateRole(int id, Role role)
        {
            var existing = await _context.Roles.FindAsync(id);

            if (existing == null) return null;

            existing.Name = role.Name;
            existing.Description = role.Description;

            await _context.SaveChangesAsync();
            return existing;
        }

        // =====================
        // DELETE
        // =====================
        public async Task<bool> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null) return false;

            // Prevent deleting seeded core roles
            if (id == 1 || id == 2 || id == 3)
                throw new Exception("Cannot delete core system roles (Admin, HR, Employee)");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}