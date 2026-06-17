using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public enum PayrollStatus
    {
        Draft,      // Generated but not approved
        Approved,   // Admin approved
        Paid        // Actually paid out
    }

    public class Payroll
    {
        [Key]
        public Guid PayrollId { get; set; } = Guid.NewGuid();

        // FK → User
        public Guid UserId { get; set; }
        public User User { get; set; }

        // FK → Organization
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        // FK → SalaryStructure used
        public Guid SalaryStructureId { get; set; }
        public SalaryStructure SalaryStructure { get; set; }

        // Period
        public int Month { get; set; }
        public int Year { get; set; }

        // =====================
        // Earnings Snapshot
        // (copied from SalaryStructure at generation time)
        // =====================
        public decimal BasicSalary { get; set; }
        public decimal HouseAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal GrossSalary { get; set; }

        // =====================
        // Attendance Data
        // =====================
        public int WorkingDaysInMonth { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int HalfDays { get; set; }
        public double TotalWorkingHours { get; set; }

        // =====================
        // Leave Data
        // =====================
        public int PaidLeaveDays { get; set; }
        public int UnpaidLeaveDays { get; set; }

        // =====================
        // Deductions
        // =====================
        public decimal AbsenceDeduction { get; set; }
        public decimal LateDeduction { get; set; }
        public decimal HalfDayDeduction { get; set; }
        public decimal UnpaidLeaveDeduction { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }

        // =====================
        // Final
        // =====================
        public decimal NetSalary { get; set; }

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

        // Who approved
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Payment details
        public DateTime? PaidAt { get; set; }
        public string? PaymentNote { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}