namespace HR_Management_System.DTOs
{
    public class LeaveResponse
    {
        public Guid LeaveId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ActionComment { get; set; }
        public DateTime? ActionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}