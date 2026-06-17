namespace HR_Management_System.DTOs
{
    public class LeaveBalanceResponse
    {
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int AnnualAllowed { get; set; } = 20;
        public int SickAllowed { get; set; } = 10;
        public int CasualAllowed { get; set; } = 7;
        public int AnnualUsed { get; set; }
        public int SickUsed { get; set; }
        public int CasualUsed { get; set; }
        public int AnnualRemaining { get; set; }
        public int SickRemaining { get; set; }
        public int CasualRemaining { get; set; }
    }
}