using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // =========================
        // GET: api/department
        // Admin + HR
        // =========================
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartments();
                return Ok(new
                {
                    message = "Departments retrieved successfully",
                    data = departments
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/department/{id}
        // Admin + HR
        // =========================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentById(id);

                if (department == null)
                    return NotFound(new { message = "Department not found" });

                return Ok(new
                {
                    message = "Department retrieved successfully",
                    data = department
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/department
        // Admin only
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DepartmentDto dto)
        {
            try
            {
                var department = await _departmentService.CreateDepartment(dto);
                return Ok(new
                {
                    message = "Department created successfully",
                    data = department
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/department/{id}
        // Admin only
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, DepartmentDto dto)
        {
            try
            {
                var department = await _departmentService.UpdateDepartment(id, dto);

                if (department == null)
                    return NotFound(new { message = "Department not found" });

                return Ok(new
                {
                    message = "Department updated successfully",
                    data = department
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/department/{id}
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _departmentService.DeleteDepartment(id);

                if (!deleted)
                    return NotFound(new { message = "Department not found" });

                return Ok(new { message = "Department deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}