using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/hr")]
    [ApiController]
    [Authorize(Roles = "HR")] // Entire controller — HR only
    public class HrController : ControllerBase
    {
        private readonly IUserManagementService _userService;
        private readonly IEmployeeService _employeeService;

        public HrController(
            IUserManagementService userService,
            IEmployeeService employeeService)
        {
            _userService = userService;
            _employeeService = employeeService;
        }

        // =========================
        // POST: api/hr/create-employee
        // HR can create employees only
        // =========================
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeUserRequest request)
        {
            try
            {
                var user = await _userService.CreateEmployee(request);
                return Ok(new
                {
                    message = "Employee created successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/hr/employees
        // HR sees only employees — not admins or other HR
        // =========================
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _userService.GetEmployees();
                return Ok(new
                {
                    message = "Employees retrieved successfully",
                    data = employees
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/hr/update-employee/{id}
        // HR can update employee basic info only
        // =========================
        [HttpPut("update-employee/{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUser(id, request);

                if (user == null)
                    return NotFound(new { message = "Employee not found" });

                // HR cannot update Admin or HR users
                if (user.Role == "Admin" || user.Role == "HR")
                    return Forbid();

                return Ok(new
                {
                    message = "Employee updated successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}