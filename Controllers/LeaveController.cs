using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/leave")]
    [ApiController]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        // =========================
        // POST: api/leave/apply
        // All roles — apply for leave
        // =========================
        [HttpPost("apply")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeave dto)
        {
            try
            {
                var result = await _leaveService.ApplyLeave(dto);
                return Ok(new
                {
                    message = "Leave applied successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/my
        // All roles — own leave history
        // =========================
        [HttpGet("my")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyLeaves()
        {
            try
            {
                var result = await _leaveService.GetMyLeaves();
                return Ok(new
                {
                    message = "Leave history retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/leave/cancel/{id}
        // All roles — cancel own pending leave
        // =========================
        [HttpPost("cancel/{id}")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> CancelLeave(Guid id)
        {
            try
            {
                await _leaveService.CancelLeave(id);
                return Ok(new { message = "Leave cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/balance/my
        // All roles — own leave balance
        // =========================
        [HttpGet("balance/my")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            try
            {
                var result = await _leaveService.GetMyLeaveBalance();
                return Ok(new
                {
                    message = "Leave balance retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/{id}
        // All roles — get leave by ID
        // Service handles ownership check
        // =========================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetLeaveById(Guid id)
        {
            try
            {
                var result = await _leaveService.GetLeaveById(id);

                if (result == null)
                    return NotFound(new { message = "Leave not found" });

                return Ok(new
                {
                    message = "Leave retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/all
        // Admin + HR only
        // =========================
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAllLeaves()
        {
            try
            {
                var result = await _leaveService.GetAllLeaves();
                return Ok(new
                {
                    message = "All leaves retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/pending
        // Admin + HR only — action required
        // =========================
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetPendingLeaves()
        {
            try
            {
                var result = await _leaveService.GetPendingLeaves();
                return Ok(new
                {
                    message = "Pending leaves retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/user/{userId}
        // Admin + HR — specific employee leaves
        // =========================
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetLeavesByUser(Guid userId)
        {
            try
            {
                var result = await _leaveService.GetLeavesByUser(userId);
                return Ok(new
                {
                    message = "User leaves retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/leave/approve/{id}
        // Admin + HR only
        // =========================
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ApproveLeave(Guid id, [FromBody] LeaveAction dto)
        {
            try
            {
                await _leaveService.ApproveLeave(id, dto);
                return Ok(new { message = "Leave approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/leave/reject/{id}
        // Admin + HR only
        // =========================
        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> RejectLeave(Guid id, [FromBody] LeaveAction dto)
        {
            try
            {
                await _leaveService.RejectLeave(id, dto);
                return Ok(new { message = "Leave rejected successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/balance/{userId}
        // Admin + HR — specific employee balance
        // =========================
        [HttpGet("balance/{userId}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetLeaveBalanceByUser(Guid userId)
        {
            try
            {
                var result = await _leaveService.GetLeaveBalanceByUser(userId);
                return Ok(new
                {
                    message = "Leave balance retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/summary
        // Admin + HR — dashboard overview
        // =========================
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetLeaveSummary()
        {
            try
            {
                var result = await _leaveService.GetLeaveSummary();
                return Ok(new
                {
                    message = "Leave summary retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/report/monthly?month=6&year=2026
        // Admin + HR only
        // =========================
        [HttpGet("report/monthly")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest(new { message = "Month must be between 1 and 12" });

                if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                    return BadRequest(new { message = "Invalid year" });

                var result = await _leaveService.GetMonthlyReport(month, year);
                return Ok(new
                {
                    message = $"Monthly leave report for {month}/{year}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/leave/report/yearly?year=2026
        // Admin + HR only
        // =========================
        [HttpGet("report/yearly")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
        {
            try
            {
                if (year < 2000 || year > DateTime.UtcNow.Year + 1)
                    return BadRequest(new { message = "Invalid year" });

                var result = await _leaveService.GetYearlyReport(year);
                return Ok(new
                {
                    message = $"Yearly leave report for {year}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/leave/{id}
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLeave(Guid id)
        {
            try
            {
                var deleted = await _leaveService.DeleteLeave(id);

                if (!deleted)
                    return NotFound(new { message = "Leave not found" });

                return Ok(new { message = "Leave deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}