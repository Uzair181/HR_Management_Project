using HR_Management_System.DTOs;
using Microsoft.AspNetCore.Http;

namespace HR_Management_System.Interfaces
{
    public interface IDocumentService
    {
        // Admin + HR
        Task<DocumentResponse> UploadDocument(UploadDocumentDto dto, IFormFile file);
        Task<DocumentResponse?> UpdateDocument(Guid id, UpdateDocumentDto dto);
        Task<List<DocumentResponse>> GetAllDocuments();
        Task<List<DocumentResponse>> GetUserDocuments(Guid userId);
        Task<bool> DeleteDocument(Guid id);

        // All roles — own documents only
        Task<List<DocumentResponse>> GetMyDocuments();
        Task<DocumentResponse?> GetDocumentById(Guid id);
        Task<(byte[] fileBytes, string fileName, string contentType)?> DownloadDocument(Guid id);
    }
}