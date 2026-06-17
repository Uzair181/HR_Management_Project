using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public enum LeaveType
    {
        Annual,
        Sick,
        Casual,
        Unpaid,
        Maternity,
        Paternity
    }

    public class Leave
    {
        [Key]
        public Guid LeaveId { get; set; } = Guid.NewGuid();

        // FK → User (who applied)
        public Guid UserId { get; set; }
        public User User { get; set; }

        // FK → Organization (multi-tenancy)
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public LeaveType Type { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Auto-calculated
        public int TotalDays { get; set; }

        public string? Reason { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        // Who approved/rejected
        public Guid? ActionBy { get; set; }
        public string? ActionComment { get; set; }
        public DateTime? ActionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}