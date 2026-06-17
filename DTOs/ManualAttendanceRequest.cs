namespace HR_Management_System.DTOs
{
    public class ManualAttendanceRequest
    {
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? Notes { get; set; }
    }
}