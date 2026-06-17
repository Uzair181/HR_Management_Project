using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // FK → Role (Role stays int because it's seeded with fixed IDs)
        public int RoleId { get; set; }
        public Role Role { get; set; }

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}