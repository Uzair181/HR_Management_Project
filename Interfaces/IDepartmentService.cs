using HR_Management_System.DTOs;
using HR_Management_System.Models;

namespace HR_Management_System.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllDepartments();
        Task<Department?> GetDepartmentById(Guid id);      // int → Guid
        Task<Department> CreateDepartment(DepartmentDto dto);
        Task<Department?> UpdateDepartment(Guid id, DepartmentDto dto);  // int → Guid
        Task<bool> DeleteDepartment(Guid id);               // int → Guid
    }
}