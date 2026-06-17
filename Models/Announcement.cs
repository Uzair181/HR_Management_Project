using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public enum AnnouncementPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum AnnouncementTarget
    {
        All,        // Everyone in org
        HR,         // HR only
        Employee    // Employees only
    }

    public class Announcement
    {
        [Key]
        public Guid AnnouncementId { get; set; } = Guid.NewGuid();

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        // FK → Created by
        public Guid CreatedByUserId { get; set; }
        public User CreatedBy { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Medium;

        // Who can see this announcement
        public AnnouncementTarget Target { get; set; } = AnnouncementTarget.All;

        // Expiry date — null means no expiry
        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}