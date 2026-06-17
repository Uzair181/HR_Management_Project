using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class Employee
    {
        [Key]
        public Guid EmployeeId { get; set; } = Guid.NewGuid();

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string Phone { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

        // FK → Department
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }

        // FK → Role (stays int)
        public int RoleId { get; set; }
        public Role Role { get; set; }

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}