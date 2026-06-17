using HR_Management_System.DTOs;

namespace HR_Management_System.Interfaces
{
    public interface IPayrollService
    {
        // Salary Structure — Admin only
        Task<SalaryStructureResponse> SetSalaryStructure(SetSalaryStructure dto);
        Task<SalaryStructureResponse?> GetSalaryStructure(Guid userId);
        Task<List<SalaryStructureResponse>> GetAllSalaryStructures();

        // Payroll Generation — Admin only
        Task<PayrollResponse> GeneratePayroll(Guid userId, GeneratePayrollDto dto);
        Task<PayrollSummaryResponse> GeneratePayrollForAll(GeneratePayrollDto dto);

        // Payroll Actions — Admin only
        Task<PayrollResponse> ApprovePayroll(Guid payrollId, PayrollApproveDto dto);
        Task<PayrollResponse> MarkAsPaid(Guid payrollId, PayrollApproveDto dto);
        Task<bool> DeletePayroll(Guid payrollId);

        // View Payroll — Admin + HR
        Task<List<PayrollResponse>> GetAllPayrolls(int month, int year);
        Task<PayrollSummaryResponse> GetPayrollSummary(int month, int year);
        Task<List<PayrollResponse>> GetUserPayrolls(Guid userId);

        // Employee
        Task<List<PayrollResponse>> GetMyPayrolls();
        Task<PayrollResponse?> GetMyPayslip(int month, int year);
    }
}