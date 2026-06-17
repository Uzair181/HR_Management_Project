using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HR_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly HRDbContext _context;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthService(
            HRDbContext context,
            UserService userService,
            IConfiguration configuration)
        {
            _context = context;
            _userService = userService;
            _configuration = configuration;
        }

        // ======================
        // REGISTER
        // Creates: Organization + Admin User atomically
        // ======================
        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            // 1. Check if email already exists
            var existingUser = await _userService.GetUserByEmail(request.Email);
            if (existingUser != null)
                throw new Exception("Email already registered");

            // 2. Create Organization
            var organization = new Organization
            {
                Name = request.OrganizationName
            };
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync(); // Save to get OrganizationId

            // 3. Create Admin User
            // RoleId = 1 is Admin (seeded in DbContext)
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = 1, // Admin
                OrganizationId = organization.OrganizationId
            };
            await _userService.CreateUser(user);

            // 4. Load Role for token generation
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            // 5. Generate JWT Token
            var token = GenerateToken(user, organization.Name);

            return new AuthResponse
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name,
                OrganizationName = organization.Name
            };
        }

        // ======================
        // LOGIN
        // ======================
        public async Task<AuthResponse> Login(LoginRequest request)
        {
            // 1. Find user
            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null)
                throw new Exception("Invalid credentials");

            // 2. Verify password
            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValidPassword)
                throw new Exception("Invalid credentials");

            // 3. Generate token
            var token = GenerateToken(user, user.Organization.Name);

            return new AuthResponse
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name,
                OrganizationName = user.Organization.Name
            };
        }

        // ======================
        // JWT TOKEN GENERATION
        // Contains: UserId, Email, Role, OrganizationId
        // ======================
        private string GenerateToken(User user, string organizationName)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is missing in appsettings.json");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Name,           user.FullName),
                new Claim(ClaimTypes.Role,           user.Role.Name),

                // ✅ CRITICAL for multi-tenancy
                new Claim("OrganizationId", user.OrganizationId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                                        Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}