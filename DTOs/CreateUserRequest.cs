namespace HR_Management_System.DTOs
{
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }  // 2 = HR, 3 = Employee
    }
}