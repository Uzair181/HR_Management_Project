using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // =========================
        // GET: api/employee
        // Admin + HR
        // =========================
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployees();
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
        // GET: api/employee/{id}
        // Admin + HR
        // =========================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeById(id);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                return Ok(new
                {
                    message = "Employee retrieved successfully",
                    data = employee
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/employee/me
        // Any logged-in user — own profile
        // =========================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var profile = await _employeeService.GetMyProfile();

                if (profile == null)
                    return NotFound(new { message = "Profile not found" });

                return Ok(new
                {
                    message = "Profile retrieved successfully",
                    data = profile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/employee
        // Admin + HR
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create(EmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.CreateEmployee(dto);
                return Ok(new
                {
                    message = "Employee created successfully",
                    data = employee
                });
            }
            catch (DbUpdateException ex)
            {
                // Shows the real database error
                return BadRequest(new
                {
                    message = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/employee/{id}
        // Admin + HR
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Update(Guid id, EmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.UpdateEmployee(id, dto);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                return Ok(new
                {
                    message = "Employee updated successfully",
                    data = employee
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/employee/{id}
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _employeeService.DeleteEmployee(id);

                if (!deleted)
                    return NotFound(new { message = "Employee not found" });

                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}