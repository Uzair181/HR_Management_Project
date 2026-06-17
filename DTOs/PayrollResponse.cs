namespace HR_Management_System.DTOs
{
    public class PayrollResponse
    {
        public Guid PayrollId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }

        // Earnings
        public decimal BasicSalary { get; set; }
        public decimal HouseAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal GrossSalary { get; set; }

        // Attendance
        public int WorkingDaysInMonth { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int HalfDays { get; set; }
        public double TotalWorkingHours { get; set; }

        // Leaves
        public int PaidLeaveDays { get; set; }
        public int UnpaidLeaveDays { get; set; }

        // Deductions
        public decimal AbsenceDeduction { get; set; }
        public decimal LateDeduction { get; set; }
        public decimal HalfDayDeduction { get; set; }
        public decimal UnpaidLeaveDeduction { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }

        // Final
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentNote { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}