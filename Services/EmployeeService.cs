using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        public EmployeeService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =====================
        // GET ALL
        // =====================
        public async Task<List<Employee>> GetAllEmployees()
        {
            var orgId = _claims.GetOrganizationId();

            return await _context.Employees
                .Where(e => e.OrganizationId == orgId)
                .Include(e => e.Department)
                .Include(e => e.Role)
                .ToListAsync();
        }

        // =====================
        // GET BY ID
        // =====================
        public async Task<Employee?> GetEmployeeById(Guid id)  // int → Guid
        {
            var orgId = _claims.GetOrganizationId();

            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.EmployeeId == id
                                       && e.OrganizationId == orgId);
        }

        // =====================
        // GET MY PROFILE
        // =====================
        public async Task<Employee?> GetMyProfile()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            // Find the user first to get their email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId
                                       && u.OrganizationId == orgId);

            if (user == null) return null;

            // Match employee by same email
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Email == user.Email
                                       && e.OrganizationId == orgId);
        }

        // =====================
        // CREATE
        // =====================
        public async Task<Employee> CreateEmployee(EmployeeDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                DateOfBirth = dto.DateOfBirth,
                DepartmentId = dto.DepartmentId,
                RoleId = dto.RoleId,
                OrganizationId = orgId,
                JoiningDate = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        // =====================
        // UPDATE
        // =====================
        public async Task<Employee?> UpdateEmployee(Guid id, EmployeeDto dto)  // int → Guid
        {
            var orgId = _claims.GetOrganizationId();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id
                                       && e.OrganizationId == orgId);

            if (employee == null) return null;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.DepartmentId = dto.DepartmentId;
            employee.RoleId = dto.RoleId;

            await _context.SaveChangesAsync();
            return employee;
        }

        // =====================
        // DELETE
        // =====================
        public async Task<bool> DeleteEmployee(Guid id)  // int → Guid
        {
            var orgId = _claims.GetOrganizationId();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id
                                       && e.OrganizationId == orgId);

            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}