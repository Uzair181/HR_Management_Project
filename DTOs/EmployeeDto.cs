namespace HR_Management_System.DTOs
{
    public class EmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public Guid DepartmentId { get; set; }  // changed to Guid
        public int RoleId { get; set; }          // stays int
    }
}