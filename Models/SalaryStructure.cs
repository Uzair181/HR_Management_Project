using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class SalaryStructure
    {
        [Key]
        public Guid SalaryStructureId { get; set; } = Guid.NewGuid();

        // FK → User
        public Guid UserId { get; set; }
        public User User { get; set; }

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        // =====================
        // Earnings
        // =====================
        public decimal BasicSalary { get; set; }
        public decimal HouseAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }

        // =====================
        // Deduction Settings
        // =====================

        // Tax percentage (e.g. 10 = 10%)
        public decimal TaxPercentage { get; set; }

        // Per day deduction for absence
        // Usually BasicSalary / WorkingDaysInMonth
        public decimal PerDayRate { get; set; }

        // Is this structure currently active
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}