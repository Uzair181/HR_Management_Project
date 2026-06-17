using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Entire controller — Admin only
    public class AdminController : ControllerBase
    {
        private readonly IUserManagementService _userService;

        public AdminController(IUserManagementService userService)
        {
            _userService = userService;
        }

        // =========================
        // POST: api/admin/create-hr
        // =========================
        [HttpPost("create-hr")]
        public async Task<IActionResult> CreateHr([FromBody] CreateHrRequest request)
        {
            try
            {
                var user = await _userService.CreateHr(request);
                return Ok(new
                {
                    message = "HR user created successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/admin/create-employee
        // =========================
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeUserRequest request)
        {
            try
            {
                var user = await _userService.CreateEmployee(request);
                return Ok(new
                {
                    message = "Employee user created successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/admin/users
        // =========================
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(new
                {
                    message = "Users retrieved successfully",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/admin/users/{id}
        // =========================
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(new
                {
                    message = "User retrieved successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/admin/users/{id}
        // =========================
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUser(id, request);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(new
                {
                    message = "User updated successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/admin/users/{id}
        // =========================
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var deleted = await _userService.DeleteUser(id);

                if (!deleted)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}