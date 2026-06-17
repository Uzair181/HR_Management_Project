using HR_Management_System.DTOs;

namespace HR_Management_System.Interfaces
{
    public interface IUserManagementService
    {
        // Admin only
        Task<UserResponse> CreateHr(CreateHrRequest request);
        Task<UserResponse> CreateEmployee(CreateEmployeeUserRequest request);
        Task<List<UserResponse>> GetAllUsers();
        Task<UserResponse?> GetUserById(Guid id);
        Task<UserResponse?> UpdateUser(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUser(Guid id);

        // HR only
        Task<List<UserResponse>> GetEmployees();
    }
}