namespace HR_Management_System.DTOs
{
    public class ProfileResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Role { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}