namespace HR_Management_System.DTOs
{
    public class GeneratePayrollDto
    {
        public int Month { get; set; }
        public int Year { get; set; }

        // Optional: override other deductions
        public decimal OtherDeductions { get; set; } = 0;
        public string? Notes { get; set; }
    }
}