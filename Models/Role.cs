using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    // Role ID stays int because we seed fixed values (1, 2, 3)
    // No point making seeded data use GUIDs
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}