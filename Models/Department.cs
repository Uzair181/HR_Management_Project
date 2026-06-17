using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class Department
    {
        [Key]
        public Guid DepartmentId { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public ICollection<Employee> Employees { get; set; }
    }
}