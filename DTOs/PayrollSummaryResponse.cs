namespace HR_Management_System.DTOs
{
    public class PayrollSummaryResponse
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public int DraftCount { get; set; }
        public int ApprovedCount { get; set; }
        public int PaidCount { get; set; }
        public List<PayrollResponse> Payrolls { get; set; } = new();
    }
}