using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public enum DocumentType
    {
        Contract,
        Certificate,
        IdCard,
        Resume,
        PaySlip,
        OfferLetter,
        WarningLetter,
        Other
    }

    public class Document
    {
        [Key]
        public Guid DocumentId { get; set; } = Guid.NewGuid();

        // FK → User (who the document belongs to)
        public Guid UserId { get; set; }
        public User User { get; set; }

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        // FK → Uploaded by
        public Guid UploadedByUserId { get; set; }
        public User UploadedBy { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DocumentType Type { get; set; } = DocumentType.Other;

        // File info
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; // pdf, docx, jpg etc
        public long FileSizeInBytes { get; set; }

        // Visibility
        public bool IsVisibleToEmployee { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}