namespace HR_Management_System.DTOs
{
    public class SetSalaryStructure
    {
        public Guid UserId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HouseAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}