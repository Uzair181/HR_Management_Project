using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        public DepartmentService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =====================
        // GET ALL — filtered by org
        // =====================
        public async Task<List<Department>> GetAllDepartments()
        {
            var orgId = _claims.GetOrganizationId();

            return await _context.Departments
                .Where(d => d.OrganizationId == orgId)
                .ToListAsync();
        }

        // =====================
        // GET BY ID — org check
        // =====================
        public async Task<Department?> GetDepartmentById(Guid id)  // int → Guid
        {
            var orgId = _claims.GetOrganizationId();

            return await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id
                                       && d.OrganizationId == orgId);
        }

        // =====================
        // CREATE
        // =====================
        public async Task<Department> CreateDepartment(DepartmentDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                OrganizationId = orgId
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return department;
        }

        // =====================
        // UPDATE
        // =====================
        public async Task<Department?> UpdateDepartment(Guid id, DepartmentDto dto)  // int → Guid
        {
            var orgId = _claims.GetOrganizationId();

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id
                                       && d.OrganizationId == orgId);

            if (department == null) return null;

            department.Name = dto.Name;
            department.Description = dto.Description;

            await _context.SaveChangesAsync();
            return department;
        }

        // =====================
        // DELETE
        // =====================
        public async Task<bool> DeleteDepartment(Guid id)  // int → Guid
        {
            var orgId = _claims.GetOrganizationId();

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id
                                       && d.OrganizationId == orgId);

            if (department == null) return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}