using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class ProfileService : IProfileService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        public ProfileService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =============================================
        // GET MY PROFILE
        // Any logged-in user sees their own profile
        // UserId always from JWT — never from request
        // =============================================
        public async Task<ProfileResponse> GetMyProfile()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == userId
                                       && u.OrganizationId == orgId);

            if (user == null)
                throw new Exception("User not found");

            return MapToResponse(user);
        }

        // =============================================
        // UPDATE MY PROFILE
        // User updates own info only
        // Cannot change role or organization
        // Email uniqueness checked before update
        // =============================================
        public async Task<ProfileResponse> UpdateMyProfile(UpdateProfile dto)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == userId
                                       && u.OrganizationId == orgId);

            if (user == null)
                throw new Exception("User not found");

            // Check email not taken by another user
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == dto.Email
                            && u.UserId != userId);

            if (emailTaken)
                throw new Exception("Email already in use by another user");

            // Update only allowed fields
            // Role and OrganizationId cannot be changed here
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Address = dto.Address;
            user.DateOfBirth = dto.DateOfBirth;

            await _context.SaveChangesAsync();

            return MapToResponse(user);
        }

        // =============================================
        // CHANGE PASSWORD
        // Verifies current password before changing
        // New password and confirm must match
        // =============================================
        public async Task ChangePassword(ChangePassword dto)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId
                                       && u.OrganizationId == orgId);

            if (user == null)
                throw new Exception("User not found");

            // Verify current password
            var isValid = BCrypt.Net.BCrypt.Verify(
                dto.CurrentPassword,
                user.PasswordHash);

            if (!isValid)
                throw new Exception("Current password is incorrect");

            // New password and confirm must match
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new Exception("New password and confirm password do not match");

            // Prevent using same password
            var isSamePassword = BCrypt.Net.BCrypt.Verify(
                dto.NewPassword,
                user.PasswordHash);

            if (isSamePassword)
                throw new Exception("New password cannot be same as current password");

            // Minimum password length
            if (dto.NewPassword.Length < 6)
                throw new Exception("Password must be at least 6 characters");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();
        }

        // =============================================
        // PRIVATE: Map User → ProfileResponse
        // Never expose PasswordHash
        // =============================================
        private ProfileResponse MapToResponse(Models.User user)
        {
            return new ProfileResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role?.Name ?? string.Empty,
                OrganizationName = user.Organization?.Name ?? string.Empty,
                CreatedAt = user.CreatedAt
            };
        }
    }
}