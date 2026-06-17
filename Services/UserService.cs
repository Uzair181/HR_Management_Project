using HR_Management_System.Data;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class UserService
    {
        private readonly HRDbContext _context;

        public UserService(HRDbContext context)
        {
            _context = context;
        }

        // =====================
        // Get by Email
        // =====================
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // =====================
        // Create User
        // =====================
        public async Task<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}