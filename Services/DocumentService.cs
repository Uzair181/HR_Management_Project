using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;
        private readonly IWebHostEnvironment _environment;

        // Allowed file types
        private readonly string[] _allowedTypes = {
            ".pdf", ".doc", ".docx", ".jpg",
            ".jpeg", ".png", ".xlsx", ".xls"
        };

        // Max file size: 10MB
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        public DocumentService(
            HRDbContext context,
            IClaimsService claims,
            IWebHostEnvironment environment)
        {
            _context = context;
            _claims = claims;
            _environment = environment;
        }

        // =============================================
        // UPLOAD DOCUMENT
        // HR/Admin uploads file for an employee
        // File saved to server, path stored in DB
        // =============================================
        public async Task<DocumentResponse> UploadDocument(
            UploadDocumentDto dto, IFormFile file)
        {
            var uploadedBy = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            // Verify target user in same org
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == dto.UserId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            // Validate file
            if (file == null || file.Length == 0)
                throw new Exception("No file provided");

            if (file.Length > MaxFileSizeBytes)
                throw new Exception("File size cannot exceed 10MB");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedTypes.Contains(extension))
                throw new Exception($"File type not allowed. Allowed: {string.Join(", ", _allowedTypes)}");

            if (!Enum.TryParse<DocumentType>(dto.Type, true, out var docType))
                throw new Exception($"Invalid document type. Valid: {string.Join(", ", Enum.GetNames<DocumentType>())}");

            // =====================
            // Save file to disk
            // =====================
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath ?? "wwwroot",
                "uploads",
                "documents",
                orgId.ToString());

            Directory.CreateDirectory(uploadsFolder);

            // Unique filename to prevent collisions
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                UserId = dto.UserId,
                OrganizationId = orgId,
                UploadedByUserId = uploadedBy,
                Title = dto.Title,
                Description = dto.Description,
                Type = docType,
                FileName = file.FileName,
                FilePath = filePath,
                FileType = extension.TrimStart('.'),
                FileSizeInBytes = file.Length,
                IsVisibleToEmployee = dto.IsVisibleToEmployee
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await _context.Entry(document).Reference(d => d.User).LoadAsync();
            await _context.Entry(document).Reference(d => d.UploadedBy).LoadAsync();

            return MapToResponse(document);
        }

        // =============================================
        // UPDATE DOCUMENT METADATA
        // HR/Admin updates title, description, visibility
        // File itself cannot be changed — re-upload instead
        // =============================================
        public async Task<DocumentResponse?> UpdateDocument(Guid id, UpdateDocumentDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.UploadedBy)
                .FirstOrDefaultAsync(d => d.DocumentId == id
                                       && d.OrganizationId == orgId);

            if (document == null) return null;

            document.Title = dto.Title;
            document.Description = dto.Description;
            document.IsVisibleToEmployee = dto.IsVisibleToEmployee;

            await _context.SaveChangesAsync();

            return MapToResponse(document);
        }

        // =============================================
        // GET ALL DOCUMENTS
        // Admin + HR sees all org documents
        // =============================================
        public async Task<List<DocumentResponse>> GetAllDocuments()
        {
            var orgId = _claims.GetOrganizationId();

            var documents = await _context.Documents
                .Where(d => d.OrganizationId == orgId)
                .Include(d => d.User)
                .Include(d => d.UploadedBy)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return documents.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET USER DOCUMENTS
        // Admin + HR views specific employee documents
        // =============================================
        public async Task<List<DocumentResponse>> GetUserDocuments(Guid userId)
        {
            var orgId = _claims.GetOrganizationId();

            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            var documents = await _context.Documents
                .Where(d => d.UserId == userId
                         && d.OrganizationId == orgId)
                .Include(d => d.User)
                .Include(d => d.UploadedBy)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return documents.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET MY DOCUMENTS
        // Employee sees own docs visible to them only
        // IsVisibleToEmployee must be true
        // =============================================
        public async Task<List<DocumentResponse>> GetMyDocuments()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();
            var role = _claims.GetRole();

            var query = _context.Documents
                .Where(d => d.UserId == userId
                         && d.OrganizationId == orgId);

            // Employee only sees visible documents
            // Admin and HR see all their own documents
            if (role == "Employee")
                query = query.Where(d => d.IsVisibleToEmployee);

            var documents = await query
                .Include(d => d.User)
                .Include(d => d.UploadedBy)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return documents.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET DOCUMENT BY ID
        // Employees can only see own visible documents
        // Admin/HR can see any org document
        // =============================================
        public async Task<DocumentResponse?> GetDocumentById(Guid id)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();
            var role = _claims.GetRole();

            var document = await _context.Documents
                .Include(d => d.User)
                .Include(d => d.UploadedBy)
                .FirstOrDefaultAsync(d => d.DocumentId == id
                                       && d.OrganizationId == orgId);

            if (document == null) return null;

            // Employee can only access own visible documents
            if (role == "Employee")
            {
                if (document.UserId != userId || !document.IsVisibleToEmployee)
                    throw new UnauthorizedAccessException("Access denied");
            }

            return MapToResponse(document);
        }

        // =============================================
        // DOWNLOAD DOCUMENT
        // Returns file bytes for download
        // Same access rules as GetDocumentById
        // =============================================
        public async Task<(byte[] fileBytes, string fileName, string contentType)?> DownloadDocument(Guid id)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();
            var role = _claims.GetRole();

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.DocumentId == id
                                       && d.OrganizationId == orgId);

            if (document == null) return null;

            // Employee access check
            if (role == "Employee")
            {
                if (document.UserId != userId || !document.IsVisibleToEmployee)
                    throw new UnauthorizedAccessException("Access denied");
            }

            if (!File.Exists(document.FilePath))
                throw new Exception("File not found on server");

            var fileBytes = await File.ReadAllBytesAsync(document.FilePath);
            var contentType = GetContentType(document.FileType);

            return (fileBytes, document.FileName, contentType);
        }

        // =============================================
        // DELETE DOCUMENT
        // Admin only — deletes file from disk + DB
        // =============================================
        public async Task<bool> DeleteDocument(Guid id)
        {
            var orgId = _claims.GetOrganizationId();

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.DocumentId == id
                                       && d.OrganizationId == orgId);

            if (document == null) return false;

            // Delete physical file
            if (File.Exists(document.FilePath))
                File.Delete(document.FilePath);

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // PRIVATE: Format file size
        // =============================================
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024):F1} MB";
        }

        // =============================================
        // PRIVATE: Get content type from extension
        // =============================================
        private string GetContentType(string fileType)
        {
            return fileType.ToLower() switch
            {
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        // =============================================
        // PRIVATE: Map Document → Response
        // =============================================
        private DocumentResponse MapToResponse(Document d)
        {
            return new DocumentResponse
            {
                DocumentId = d.DocumentId,
                UserId = d.UserId,
                UserFullName = d.User?.FullName ?? string.Empty,
                Title = d.Title,
                Description = d.Description,
                Type = d.Type.ToString(),
                FileName = d.FileName,
                FileType = d.FileType,
                FileSizeFormatted = FormatFileSize(d.FileSizeInBytes),
                IsVisibleToEmployee = d.IsVisibleToEmployee,
                UploadedByName = d.UploadedBy?.FullName ?? string.Empty,
                CreatedAt = d.CreatedAt
            };
        }
    }
}