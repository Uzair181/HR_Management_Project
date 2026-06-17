using HR_Management_System.DTOs;
using HR_Management_System.Models;

namespace HR_Management_System.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllEmployees();
        Task<Employee?> GetEmployeeById(Guid id);      // int → Guid
        Task<Employee> CreateEmployee(EmployeeDto dto);
        Task<Employee?> UpdateEmployee(Guid id, EmployeeDto dto);  // int → Guid
        Task<bool> DeleteEmployee(Guid id);             // int → Guid
        Task<Employee?> GetMyProfile();
    }
}