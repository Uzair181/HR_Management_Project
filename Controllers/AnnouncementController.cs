using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/announcement")]
    [ApiController]
    [Authorize]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        // =========================
        // POST: api/announcement
        // Admin + HR
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncement dto)
        {
            try
            {
                var result = await _announcementService.CreateAnnouncement(dto);
                return Ok(new
                {
                    message = "Announcement created successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/announcement/{id}
        // Admin + HR
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementDto dto)
        {
            try
            {
                var result = await _announcementService.UpdateAnnouncement(id, dto);

                if (result == null)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(new
                {
                    message = "Announcement updated successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/announcement/all
        // Admin + HR — includes inactive
        // =========================
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _announcementService.GetAllAnnouncements();
                return Ok(new
                {
                    message = "Announcements retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/announcement/my
        // All roles — filtered by role + active only
        // =========================
        [HttpGet("my")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyAnnouncements()
        {
            try
            {
                var result = await _announcementService.GetMyAnnouncements();
                return Ok(new
                {
                    message = "Announcements retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/announcement/{id}
        // All roles
        // =========================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _announcementService.GetAnnouncementById(id);

                if (result == null)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(new
                {
                    message = "Announcement retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PATCH: api/announcement/{id}/toggle
        // Admin only
        // =========================
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            try
            {
                var result = await _announcementService.ToggleActive(id);

                if (!result)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(new { message = "Announcement status toggled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/announcement/{id}
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _announcementService.DeleteAnnouncement(id);

                if (!deleted)
                    return NotFound(new { message = "Announcement not found" });

                return Ok(new { message = "Announcement deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}