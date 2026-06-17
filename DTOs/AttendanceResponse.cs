namespace HR_Management_System.DTOs
{
    public class AttendanceResponse
    {
        public Guid AttendanceId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public double WorkingHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}