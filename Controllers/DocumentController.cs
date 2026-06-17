using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Route("api/document")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // =========================
        // POST: api/document/upload
        // Admin + HR — multipart form
        // =========================
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Upload(
            [FromForm] UploadDocumentDto dto,
            IFormFile file)
        {
            try
            {
                var result = await _documentService.UploadDocument(dto, file);
                return Ok(new
                {
                    message = "Document uploaded successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // PUT: api/document/{id}
        // Admin + HR
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentDto dto)
        {
            try
            {
                var result = await _documentService.UpdateDocument(id, dto);

                if (result == null)
                    return NotFound(new { message = "Document not found" });

                return Ok(new
                {
                    message = "Document updated successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/document/all
        // Admin + HR
        // =========================
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _documentService.GetAllDocuments();
                return Ok(new
                {
                    message = "Documents retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/document/user/{userId}
        // Admin + HR
        // =========================
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetUserDocuments(Guid userId)
        {
            try
            {
                var result = await _documentService.GetUserDocuments(userId);
                return Ok(new
                {
                    message = "User documents retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/document/my
        // All roles — own documents only
        // =========================
        [HttpGet("my")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetMyDocuments()
        {
            try
            {
                var result = await _documentService.GetMyDocuments();
                return Ok(new
                {
                    message = "Documents retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/document/{id}
        // All roles — access rules in service
        // =========================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _documentService.GetDocumentById(id);

                if (result == null)
                    return NotFound(new { message = "Document not found" });

                return Ok(new
                {
                    message = "Document retrieved successfully",
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // GET: api/document/{id}/download
        // All roles — access rules in service
        // =========================
        [HttpGet("{id}/download")]
        [Authorize(Roles = "Admin,HR,Employee")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var result = await _documentService.DownloadDocument(id);

                if (result == null)
                    return NotFound(new { message = "Document not found" });

                var (fileBytes, fileName, contentType) = result.Value;

                return File(fileBytes, contentType, fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // DELETE: api/document/{id}
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _documentService.DeleteDocument(id);

                if (!deleted)
                    return NotFound(new { message = "Document not found" });

                return Ok(new { message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}