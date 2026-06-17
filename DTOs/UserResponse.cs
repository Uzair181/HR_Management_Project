namespace HR_Management_System.DTOs
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}