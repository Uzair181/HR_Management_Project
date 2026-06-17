namespace HR_Management_System.DTOs
{
    public class UploadDocumentDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "Other";
        public bool IsVisibleToEmployee { get; set; } = true;
    }
}