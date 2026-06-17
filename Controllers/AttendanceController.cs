using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/attendance")]
    [ApiController]
    [Authorize] // All endpoints require login
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // =========================
        // POST: api/attendance/checkin
        // All roles — mark own arrival
        // =========================
        [HttpPost("checkin")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
        {
            try
            {
                var result = await _attendanceService.CheckIn(request);
                return Ok(new
                {
                    message = "Checked in successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/attendance/checkout
        // All roles — mark own departure
        // =========================
        [HttpPost("checkout")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
        {
            try
            {
                var result = await _attendanceService.CheckOut(request);
                return Ok(new
                {
                    message = "Checked out successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/attendance/my
        // All roles — view own attendance history
        // =========================
        [HttpGet("my")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyAttendance()
        {
            try
            {
                var result = await _attendanceService.GetMyAttendance();
                return Ok(new
                {
                    message = "Attendance retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/attendance/status?date=2026-06-17
        // All roles — check own status for a date
        // =========================
        [HttpGet("status")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyStatusByDate([FromQuery] DateTime date)
        {
            try
            {
                var result = await _attendanceService.GetMyStatusByDate(date);

                if (result == null)
                    return Ok(new
                    {
                        message = "No attendance record found for this date",
                        data = (object?)null
                    });

                return Ok(new
                {
                    message = "Status retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/attendance/all
        // Admin + HR only
        // =========================
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAllAttendance()
        {
            try
            {
                var result = await _attendanceService.GetAllAttendance();
                return Ok(new
                {
                    message = "All attendance retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/attendance/user/{userId}
        // Admin + HR only
        // =========================
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetUserAttendance(Guid userId)
        {
            try
            {
                var result = await _attendanceService.GetUserAttendance(userId);
                return Ok(new
                {
                    message = "User attendance retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // POST: api/attendance/manual
        // Admin + HR only
        // =========================
        [HttpPost("manual")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> AddManualAttendance([FromBody] ManualAttendanceRequest request)
        {
            try
            {
                var result = await _attendanceService.AddManualAttendance(request);
                return Ok(new
                {
                    message = "Manual attendance added successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/attendance/{id}
        // Admin + HR only
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UpdateAttendance(Guid id, [FromBody] UpdateAttendanceRequest request)
        {
            try
            {
                var result = await _attendanceService.UpdateAttendance(id, request);

                if (result == null)
                    return NotFound(new { message = "Attendance record not found" });

                return Ok(new
                {
                    message = "Attendance updated successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/attendance/{id}
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttendance(Guid id)
        {
            try
            {
                var deleted = await _attendanceService.DeleteAttendance(id);

                if (!deleted)
                    return NotFound(new { message = "Attendance record not found" });

                return Ok(new { message = "Attendance deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/attendance/report/monthly?month=6&year=2026
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

                if (year < 2000 || year > DateTime.UtcNow.Year)
                    return BadRequest(new { message = "Invalid year" });

                var result = await _attendanceService.GetMonthlyReport(month, year);
                return Ok(new
                {
                    message = $"Monthly report for {month}/{year}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/attendance/daily-summary?date=2026-06-17
        // Admin + HR only
        // =========================
        [HttpGet("daily-summary")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetDailySummary([FromQuery] DateTime date)
        {
            try
            {
                var result = await _attendanceService.GetDailySummary(date);
                return Ok(new
                {
                    message = $"Daily summary for {date:yyyy-MM-dd}",
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