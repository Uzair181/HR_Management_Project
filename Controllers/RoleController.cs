using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // =========================
        // GET: api/role
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllRoles();
                return Ok(new
                {
                    message = "Roles retrieved successfully",
                    data = roles
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/role/{id}
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var role = await _roleService.GetRoleById(id);

                if (role == null)
                    return NotFound(new { message = "Role not found" });

                return Ok(new
                {
                    message = "Role retrieved successfully",
                    data = role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/role
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            try
            {
                var created = await _roleService.CreateRole(role);
                return Ok(new
                {
                    message = "Role created successfully",
                    data = created
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/role/{id}
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Role role)
        {
            try
            {
                var updated = await _roleService.UpdateRole(id, role);

                if (updated == null)
                    return NotFound(new { message = "Role not found" });

                return Ok(new
                {
                    message = "Role updated successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/role/{id}
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _roleService.DeleteRole(id);

                if (!deleted)
                    return NotFound(new { message = "Role not found" });

                return Ok(new { message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                // Catches "Cannot delete core system roles" exception
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}