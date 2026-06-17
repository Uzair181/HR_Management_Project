namespace HR_Management_System.DTOs
{
    public class UpdateDocumentDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsVisibleToEmployee { get; set; } = true;
    }
}