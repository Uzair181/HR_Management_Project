using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.DTOs
{
    public class ManualLeave
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}