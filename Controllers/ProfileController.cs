using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // =========================
        // GET: api/profile/me
        // All roles
        // =========================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var result = await _profileService.GetMyProfile();
                return Ok(new
                {
                    message = "Profile retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/profile/update
        // All roles
        // =========================
        [HttpPut("update")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfile dto)
        {
            try
            {
                var result = await _profileService.UpdateMyProfile(dto);
                return Ok(new
                {
                    message = "Profile updated successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/profile/change-password
        // All roles
        // =========================
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword dto)
        {
            try
            {
                await _profileService.ChangePassword(dto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}