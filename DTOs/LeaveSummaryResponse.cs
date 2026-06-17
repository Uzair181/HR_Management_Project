namespace HR_Management_System.DTOs
{
    public class LeaveSummaryResponse
    {
        public int TotalPending { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalCancelled { get; set; }
        public List<LeaveResponse> RecentLeaves { get; set; } = new();
    }
}