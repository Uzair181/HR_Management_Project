using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public enum AttendanceStatus
    {
        Present,
        Late,
        HalfDay,
        Absent
    }

    public class Attendance
    {
        [Key]
        public Guid AttendanceId { get; set; } = Guid.NewGuid();

        // FK → User
        public Guid UserId { get; set; }
        public User User { get; set; }

        // FK → Organization (multi-tenancy)
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        // Date of attendance (just the date, no time)
        public DateTime Date { get; set; }

        // Check-in and Check-out times
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        // Auto-calculated when checkout happens
        public double WorkingHours { get; set; } = 0;

        // Auto-set based on checkin time and working hours
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;

        // Optional HR note
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}