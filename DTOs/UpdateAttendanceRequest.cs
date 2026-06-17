namespace HR_Management_System.DTOs
{
    public class UpdateAttendanceRequest
    {
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? Notes { get; set; }
    }
}