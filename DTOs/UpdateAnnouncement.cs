namespace HR_Management_System.DTOs
{
    public class UpdateAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Target { get; set; } = "All";
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}