namespace HR_Management_System.DTOs
{
    public class MonthlyReportResponse
    {
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int LateDays { get; set; }
        public int HalfDays { get; set; }
        public int AbsentDays { get; set; }
        public double TotalWorkingHours { get; set; }
    }
}