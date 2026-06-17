using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        public UserManagementService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =============================================
        // ADMIN: Create HR User
        // RoleId = 2 (HR) — hardcoded, Admin cannot
        // accidentally assign Admin role
        // =============================================
        public async Task<UserResponse> CreateHr(CreateHrRequest request)
        {
            var orgId = _claims.GetOrganizationId();

            // Check duplicate email across whole system
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existing != null)
                throw new Exception("Email already registered");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = 2, // HR — hardcoded, cannot be changed by caller
                OrganizationId = orgId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load role + org for response
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            await _context.Entry(user).Reference(u => u.Organization).LoadAsync();

            return MapToResponse(user);
        }

        // =============================================
        // ADMIN + HR: Create Employee User
        // RoleId = 3 (Employee) — hardcoded
        // =============================================
        public async Task<UserResponse> CreateEmployee(CreateEmployeeUserRequest request)
        {
            var orgId = _claims.GetOrganizationId();

            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existing != null)
                throw new Exception("Email already registered");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = 3, // Employee — hardcoded
                OrganizationId = orgId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            await _context.Entry(user).Reference(u => u.Organization).LoadAsync();

            return MapToResponse(user);
        }

        // =============================================
        // ADMIN: Get All Users in Organization
        // =============================================
        public async Task<List<UserResponse>> GetAllUsers()
        {
            var orgId = _claims.GetOrganizationId();

            var users = await _context.Users
                .Where(u => u.OrganizationId == orgId)
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .ToListAsync();

            return users.Select(MapToResponse).ToList();
        }

        // =============================================
        // ADMIN: Get User By Id — org check
        // =============================================
        public async Task<UserResponse?> GetUserById(Guid id)
        {
            var orgId = _claims.GetOrganizationId();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == orgId);

            if (user == null) return null;

            return MapToResponse(user);
        }

        // =============================================
        // ADMIN: Update User — org check
        // Only FullName and Email — no role change allowed
        // =============================================
        public async Task<UserResponse?> UpdateUser(Guid id, UpdateUserRequest request)
        {
            var orgId = _claims.GetOrganizationId();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == orgId);

            if (user == null) return null;

            // Check new email not taken by another user
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == request.Email
                            && u.UserId != id);

            if (emailTaken)
                throw new Exception("Email already in use by another user");

            user.FullName = request.FullName;
            user.Email = request.Email;

            await _context.SaveChangesAsync();

            return MapToResponse(user);
        }

        // =============================================
        // ADMIN: Delete User — org check
        // Cannot delete yourself
        // =============================================
        public async Task<bool> DeleteUser(Guid id)
        {
            var orgId = _claims.GetOrganizationId();
            var loggedInUser = _claims.GetUserId();

            // Prevent self-deletion
            if (id == loggedInUser)
                throw new Exception("You cannot delete your own account");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == orgId);

            if (user == null) return false;

            // Prevent deleting another Admin
            if (user.RoleId == 1)
                throw new Exception("Cannot delete an Admin user");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // HR: Get Only Employees in Organization
        // RoleId = 3 filter — HR cannot see Admins
        // =============================================
        public async Task<List<UserResponse>> GetEmployees()
        {
            var orgId = _claims.GetOrganizationId();

            var users = await _context.Users
                .Where(u => u.OrganizationId == orgId
                         && u.RoleId == 3) // Employees only
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .ToListAsync();

            return users.Select(MapToResponse).ToList();
        }

        // =============================================
        // PRIVATE: Map User → UserResponse
        // Never expose PasswordHash to API response
        // =============================================
        private UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? string.Empty,
                OrganizationName = user.Organization?.Name ?? string.Empty,
                CreatedAt = user.CreatedAt
            };
        }
    }
}