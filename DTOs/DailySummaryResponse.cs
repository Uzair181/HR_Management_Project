namespace HR_Management_System.DTOs
{
    public class DailySummaryResponse
    {
        public DateTime Date { get; set; }
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int HalfDay { get; set; }
        public int Absent { get; set; }
        public List<AttendanceResponse> Records { get; set; } = new();
    }
}