using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/payroll")]
    [ApiController]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        // =========================
        // POST: api/payroll/salary-structure
        // Admin only — set employee salary
        // =========================
        [HttpPost("salary-structure")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetSalaryStructure([FromBody] SetSalaryStructure dto)
        {
            try
            {
                var result = await _payrollService.SetSalaryStructure(dto);
                return Ok(new
                {
                    message = "Salary structure set successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/salary-structure/all
        // Admin only
        // =========================
        [HttpGet("salary-structure/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSalaryStructures()
        {
            try
            {
                var result = await _payrollService.GetAllSalaryStructures();
                return Ok(new
                {
                    message = "Salary structures retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/salary-structure/{userId}
        // Admin only
        // =========================
        [HttpGet("salary-structure/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSalaryStructure(Guid userId)
        {
            try
            {
                var result = await _payrollService.GetSalaryStructure(userId);

                if (result == null)
                    return NotFound(new { message = "No salary structure found" });

                return Ok(new
                {
                    message = "Salary structure retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/payroll/generate/{userId}
        // Admin only — generate for one employee
        // =========================
        [HttpPost("generate/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeneratePayroll(
            Guid userId,
            [FromBody] GeneratePayrollDto dto)
        {
            try
            {
                var result = await _payrollService.GeneratePayroll(userId, dto);
                return Ok(new
                {
                    message = "Payroll generated successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/payroll/generate-all
        // Admin only — generate for entire org
        // =========================
        [HttpPost("generate-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeneratePayrollForAll([FromBody] GeneratePayrollDto dto)
        {
            try
            {
                var result = await _payrollService.GeneratePayrollForAll(dto);
                return Ok(new
                {
                    message = $"Payroll generated for {result.TotalEmployees} employees",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/payroll/approve/{id}
        // Admin only
        // =========================
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePayroll(
            Guid id,
            [FromBody] PayrollApproveDto dto)
        {
            try
            {
                var result = await _payrollService.ApprovePayroll(id, dto);
                return Ok(new
                {
                    message = "Payroll approved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/payroll/mark-paid/{id}
        // Admin only
        // =========================
        [HttpPut("mark-paid/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsPaid(
            Guid id,
            [FromBody] PayrollApproveDto dto)
        {
            try
            {
                var result = await _payrollService.MarkAsPaid(id, dto);
                return Ok(new
                {
                    message = "Payroll marked as paid",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/payroll/{id}
        // Admin only — Draft only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePayroll(Guid id)
        {
            try
            {
                var deleted = await _payrollService.DeletePayroll(id);

                if (!deleted)
                    return NotFound(new { message = "Payroll not found" });

                return Ok(new { message = "Payroll deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/all?month=6&year=2026
        // Admin + HR
        // =========================
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAllPayrolls(
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest(new { message = "Month must be between 1 and 12" });

                var result = await _payrollService.GetAllPayrolls(month, year);
                return Ok(new
                {
                    message = "Payrolls retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/summary?month=6&year=2026
        // Admin + HR
        // =========================
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetPayrollSummary(
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                var result = await _payrollService.GetPayrollSummary(month, year);
                return Ok(new
                {
                    message = "Payroll summary retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/user/{userId}
        // Admin + HR
        // =========================
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetUserPayrolls(Guid userId)
        {
            try
            {
                var result = await _payrollService.GetUserPayrolls(userId);
                return Ok(new
                {
                    message = "User payrolls retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/my
        // Employee — own payroll history
        // =========================
        [HttpGet("my")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyPayrolls()
        {
            try
            {
                var result = await _payrollService.GetMyPayrolls();
                return Ok(new
                {
                    message = "Payroll history retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/payroll/my/payslip?month=6&year=2026
        // Employee — specific month payslip
        // =========================
        [HttpGet("my/payslip")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyPayslip(
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                var result = await _payrollService.GetMyPayslip(month, year);

                if (result == null)
                    return NotFound(new
                    {
                        message = $"No payslip found for {month}/{year}"
                    });

                return Ok(new
                {
                    message = "Payslip retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}