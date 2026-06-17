using HR_Management_System.Models;

namespace HR_Management_System.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRoles();
        Task<Role?> GetRoleById(int id);
        Task<Role> CreateRole(Role role);
        Task<Role?> UpdateRole(int id, Role role);
        Task<bool> DeleteRole(int id);
    }
}