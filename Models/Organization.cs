using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class Organization
    {
        [Key]
        public Guid OrganizationId { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; }
        public ICollection<Department> Departments { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}