using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        // Late deduction = X minutes per late occurrence
        private const decimal LateDeductionPerOccurrence = 0.25m; // 15 min = 0.25 day

        public PayrollService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =============================================
        // SET SALARY STRUCTURE
        // Admin sets or updates salary for a user
        // PerDayRate auto-calculated from BasicSalary
        // =============================================
        public async Task<SalaryStructureResponse> SetSalaryStructure(SetSalaryStructure dto)
        {
            var orgId = _claims.GetOrganizationId();

            // Verify user belongs to org
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == dto.UserId
                                       && u.OrganizationId == orgId);

            if (user == null)
                throw new Exception("User not found in your organization");

            // Deactivate existing structure
            var existing = await _context.SalaryStructures
                .FirstOrDefaultAsync(s => s.UserId == dto.UserId
                                       && s.OrganizationId == orgId
                                       && s.IsActive);

            if (existing != null)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            // Standard working days in month = 26
            var workingDays = 26;
            var grossSalary = dto.BasicSalary
                            + dto.HouseAllowance
                            + dto.TransportAllowance
                            + dto.MedicalAllowance
                            + dto.OtherAllowances;

            var structure = new SalaryStructure
            {
                UserId = dto.UserId,
                OrganizationId = orgId,
                BasicSalary = dto.BasicSalary,
                HouseAllowance = dto.HouseAllowance,
                TransportAllowance = dto.TransportAllowance,
                MedicalAllowance = dto.MedicalAllowance,
                OtherAllowances = dto.OtherAllowances,
                TaxPercentage = dto.TaxPercentage,
                PerDayRate = grossSalary / workingDays,
                IsActive = true
            };

            _context.SalaryStructures.Add(structure);
            await _context.SaveChangesAsync();

            await _context.Entry(structure).Reference(s => s.User).LoadAsync();

            return MapSalaryStructureToResponse(structure);
        }

        // =============================================
        // GET SALARY STRUCTURE
        // Admin views salary setup for one employee
        // =============================================
        public async Task<SalaryStructureResponse?> GetSalaryStructure(Guid userId)
        {
            var orgId = _claims.GetOrganizationId();

            var structure = await _context.SalaryStructures
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId
                                       && s.OrganizationId == orgId
                                       && s.IsActive);

            if (structure == null) return null;

            return MapSalaryStructureToResponse(structure);
        }

        // =============================================
        // GET ALL SALARY STRUCTURES
        // Admin views all employee salary setups
        // =============================================
        public async Task<List<SalaryStructureResponse>> GetAllSalaryStructures()
        {
            var orgId = _claims.GetOrganizationId();

            var structures = await _context.SalaryStructures
                .Where(s => s.OrganizationId == orgId
                         && s.IsActive)
                .Include(s => s.User)
                .ToListAsync();

            return structures.Select(MapSalaryStructureToResponse).ToList();
        }

        // =============================================
        // GENERATE PAYROLL FOR ONE EMPLOYEE
        // Pulls attendance + leave data automatically
        // Calculates all deductions precisely
        // =============================================
        public async Task<PayrollResponse> GeneratePayroll(Guid userId, GeneratePayrollDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            // Check not already generated
            var exists = await _context.Payrolls
                .AnyAsync(p => p.UserId == userId
                            && p.OrganizationId == orgId
                            && p.Month == dto.Month
                            && p.Year == dto.Year);

            if (exists)
                throw new Exception("Payroll already generated for this employee this month");

            // Get active salary structure
            var structure = await _context.SalaryStructures
                .FirstOrDefaultAsync(s => s.UserId == userId
                                       && s.OrganizationId == orgId
                                       && s.IsActive);

            if (structure == null)
                throw new Exception("No salary structure found for this employee");

            // =====================
            // Pull Attendance Data
            // =====================
            var attendance = await _context.Attendances
                .Where(a => a.UserId == userId
                         && a.OrganizationId == orgId
                         && a.Date.Month == dto.Month
                         && a.Date.Year == dto.Year)
                .ToListAsync();

            // Standard working days in month
            var workingDaysInMonth = 26;

            var presentDays = attendance.Count(a => a.Status == AttendanceStatus.Present);
            var lateDays = attendance.Count(a => a.Status == AttendanceStatus.Late);
            var halfDays = attendance.Count(a => a.Status == AttendanceStatus.HalfDay);
            var totalMarkedDays = attendance.Count;
            var absentDays = workingDaysInMonth - totalMarkedDays;
            var totalWorkingHrs = attendance.Sum(a => a.WorkingHours);

            // =====================
            // Pull Leave Data
            // Only approved leaves count
            // =====================
            var leaves = await _context.Leaves
                .Where(l => l.UserId == userId
                         && l.OrganizationId == orgId
                         && l.Status == LeaveStatus.Approved
                         && l.FromDate.Month == dto.Month
                         && l.FromDate.Year == dto.Year)
                .ToListAsync();

            // Annual/Sick/Casual = Paid leave
            var paidLeaveDays = leaves
                .Where(l => l.Type == LeaveType.Annual
                         || l.Type == LeaveType.Sick
                         || l.Type == LeaveType.Casual
                         || l.Type == LeaveType.Maternity
                         || l.Type == LeaveType.Paternity)
                .Sum(l => l.TotalDays);

            // Unpaid = deducted from salary
            var unpaidLeaveDays = leaves
                .Where(l => l.Type == LeaveType.Unpaid)
                .Sum(l => l.TotalDays);

            // =====================
            // Calculate Earnings
            // =====================
            var grossSalary = structure.BasicSalary
                            + structure.HouseAllowance
                            + structure.TransportAllowance
                            + structure.MedicalAllowance
                            + structure.OtherAllowances;

            // =====================
            // Calculate Deductions
            // =====================

            // Absence: each absent day = PerDayRate
            // Paid leaves don't count as absent
            var effectiveAbsent = Math.Max(0, absentDays - paidLeaveDays);
            var absenceDeduction = effectiveAbsent * structure.PerDayRate;

            // Late: each late = 0.25 day deduction
            var lateDeduction = lateDays * (structure.PerDayRate * LateDeductionPerOccurrence);

            // HalfDay: each halfday = 0.5 day deduction
            var halfDayDeduction = halfDays * (structure.PerDayRate * 0.5m);

            // Unpaid leave deduction
            var unpaidLeaveDeduction = unpaidLeaveDays * structure.PerDayRate;

            // Tax on gross salary
            var taxDeduction = grossSalary * (structure.TaxPercentage / 100);

            var totalDeductions = absenceDeduction
                                   + lateDeduction
                                   + halfDayDeduction
                                   + unpaidLeaveDeduction
                                   + taxDeduction
                                   + dto.OtherDeductions;

            var netSalary = grossSalary - totalDeductions;

            // =====================
            // Save Payroll Record
            // =====================
            var payroll = new Payroll
            {
                UserId = userId,
                OrganizationId = orgId,
                SalaryStructureId = structure.SalaryStructureId,
                Month = dto.Month,
                Year = dto.Year,

                // Earnings snapshot
                BasicSalary = structure.BasicSalary,
                HouseAllowance = structure.HouseAllowance,
                TransportAllowance = structure.TransportAllowance,
                MedicalAllowance = structure.MedicalAllowance,
                OtherAllowances = structure.OtherAllowances,
                GrossSalary = grossSalary,

                // Attendance data
                WorkingDaysInMonth = workingDaysInMonth,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LateDays = lateDays,
                HalfDays = halfDays,
                TotalWorkingHours = totalWorkingHrs,

                // Leave data
                PaidLeaveDays = paidLeaveDays,
                UnpaidLeaveDays = unpaidLeaveDays,

                // Deductions
                AbsenceDeduction = absenceDeduction,
                LateDeduction = lateDeduction,
                HalfDayDeduction = halfDayDeduction,
                UnpaidLeaveDeduction = unpaidLeaveDeduction,
                TaxDeduction = taxDeduction,
                OtherDeductions = dto.OtherDeductions,
                TotalDeductions = totalDeductions,

                NetSalary = netSalary,
                Notes = dto.Notes,
                Status = PayrollStatus.Draft
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            await _context.Entry(payroll).Reference(p => p.User).LoadAsync();

            return MapPayrollToResponse(payroll);
        }

        // =============================================
        // GENERATE PAYROLL FOR ALL EMPLOYEES
        // Admin generates entire org payroll at once
        // Skips employees without salary structure
        // Skips already generated payrolls
        // =============================================
        public async Task<PayrollSummaryResponse> GeneratePayrollForAll(GeneratePayrollDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            // Get all users in org
            var users = await _context.Users
                .Where(u => u.OrganizationId == orgId)
                .ToListAsync();

            var generated = new List<PayrollResponse>();

            foreach (var user in users)
            {
                // Skip if already generated
                var exists = await _context.Payrolls
                    .AnyAsync(p => p.UserId == user.UserId
                                && p.OrganizationId == orgId
                                && p.Month == dto.Month
                                && p.Year == dto.Year);

                if (exists) continue;

                // Skip if no salary structure
                var hasStructure = await _context.SalaryStructures
                    .AnyAsync(s => s.UserId == user.UserId
                                && s.OrganizationId == orgId
                                && s.IsActive);

                if (!hasStructure) continue;

                try
                {
                    var payroll = await GeneratePayroll(user.UserId, dto);
                    generated.Add(payroll);
                }
                catch { continue; }
            }

            return new PayrollSummaryResponse
            {
                Month = dto.Month,
                Year = dto.Year,
                TotalEmployees = generated.Count,
                TotalGrossSalary = generated.Sum(p => p.GrossSalary),
                TotalDeductions = generated.Sum(p => p.TotalDeductions),
                TotalNetSalary = generated.Sum(p => p.NetSalary),
                DraftCount = generated.Count,
                ApprovedCount = 0,
                PaidCount = 0,
                Payrolls = generated
            };
        }

        // =============================================
        // APPROVE PAYROLL
        // Admin reviews and approves Draft payroll
        // =============================================
        public async Task<PayrollResponse> ApprovePayroll(Guid payrollId, PayrollApproveDto dto)
        {
            var orgId = _claims.GetOrganizationId();
            var approvedBy = _claims.GetUserId();

            var payroll = await _context.Payrolls
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId
                                       && p.OrganizationId == orgId);

            if (payroll == null)
                throw new Exception("Payroll not found");

            if (payroll.Status != PayrollStatus.Draft)
                throw new Exception("Only Draft payrolls can be approved");

            payroll.Status = PayrollStatus.Approved;
            payroll.ApprovedBy = approvedBy;
            payroll.ApprovedAt = DateTime.UtcNow;
            payroll.PaymentNote = dto.PaymentNote;

            await _context.SaveChangesAsync();

            return MapPayrollToResponse(payroll);
        }

        // =============================================
        // MARK AS PAID
        // Admin marks approved payroll as paid
        // =============================================
        public async Task<PayrollResponse> MarkAsPaid(Guid payrollId, PayrollApproveDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            var payroll = await _context.Payrolls
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId
                                       && p.OrganizationId == orgId);

            if (payroll == null)
                throw new Exception("Payroll not found");

            if (payroll.Status != PayrollStatus.Approved)
                throw new Exception("Only Approved payrolls can be marked as paid");

            payroll.Status = PayrollStatus.Paid;
            payroll.PaidAt = DateTime.UtcNow;
            payroll.PaymentNote = dto.PaymentNote;

            await _context.SaveChangesAsync();

            return MapPayrollToResponse(payroll);
        }

        // =============================================
        // DELETE PAYROLL
        // Admin deletes Draft payroll only
        // Cannot delete Approved or Paid payrolls
        // =============================================
        public async Task<bool> DeletePayroll(Guid payrollId)
        {
            var orgId = _claims.GetOrganizationId();

            var payroll = await _context.Payrolls
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId
                                       && p.OrganizationId == orgId);

            if (payroll == null) return false;

            if (payroll.Status != PayrollStatus.Draft)
                throw new Exception("Only Draft payrolls can be deleted");

            _context.Payrolls.Remove(payroll);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // GET ALL PAYROLLS — Admin + HR
        // Filtered by month + year + org
        // =============================================
        public async Task<List<PayrollResponse>> GetAllPayrolls(int month, int year)
        {
            var orgId = _claims.GetOrganizationId();

            var payrolls = await _context.Payrolls
                .Where(p => p.OrganizationId == orgId
                         && p.Month == month
                         && p.Year == year)
                .Include(p => p.User)
                .OrderBy(p => p.User.FullName)
                .ToListAsync();

            return payrolls.Select(MapPayrollToResponse).ToList();
        }

        // =============================================
        // GET PAYROLL SUMMARY — Admin + HR
        // Full month overview with totals
        // =============================================
        public async Task<PayrollSummaryResponse> GetPayrollSummary(int month, int year)
        {
            var orgId = _claims.GetOrganizationId();

            var payrolls = await _context.Payrolls
                .Where(p => p.OrganizationId == orgId
                         && p.Month == month
                         && p.Year == year)
                .Include(p => p.User)
                .ToListAsync();

            return new PayrollSummaryResponse
            {
                Month = month,
                Year = year,
                TotalEmployees = payrolls.Count,
                TotalGrossSalary = payrolls.Sum(p => p.GrossSalary),
                TotalDeductions = payrolls.Sum(p => p.TotalDeductions),
                TotalNetSalary = payrolls.Sum(p => p.NetSalary),
                DraftCount = payrolls.Count(p => p.Status == PayrollStatus.Draft),
                ApprovedCount = payrolls.Count(p => p.Status == PayrollStatus.Approved),
                PaidCount = payrolls.Count(p => p.Status == PayrollStatus.Paid),
                Payrolls = payrolls.Select(MapPayrollToResponse).ToList()
            };
        }

        // =============================================
        // GET USER PAYROLLS — Admin + HR
        // Full payroll history of one employee
        // =============================================
        public async Task<List<PayrollResponse>> GetUserPayrolls(Guid userId)
        {
            var orgId = _claims.GetOrganizationId();

            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            var payrolls = await _context.Payrolls
                .Where(p => p.UserId == userId
                         && p.OrganizationId == orgId)
                .Include(p => p.User)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();

            return payrolls.Select(MapPayrollToResponse).ToList();
        }

        // =============================================
        // GET MY PAYROLLS — Employee
        // Employee sees own payroll history
        // =============================================
        public async Task<List<PayrollResponse>> GetMyPayrolls()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var payrolls = await _context.Payrolls
                .Where(p => p.UserId == userId
                         && p.OrganizationId == orgId)
                .Include(p => p.User)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();

            return payrolls.Select(MapPayrollToResponse).ToList();
        }

        // =============================================
        // GET MY PAYSLIP — Employee
        // Employee views specific month payslip
        // =============================================
        public async Task<PayrollResponse?> GetMyPayslip(int month, int year)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var payroll = await _context.Payrolls
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId
                                       && p.OrganizationId == orgId
                                       && p.Month == month
                                       && p.Year == year);

            if (payroll == null) return null;

            return MapPayrollToResponse(payroll);
        }

        // =============================================
        // PRIVATE: Map SalaryStructure → Response
        // =============================================
        private SalaryStructureResponse MapSalaryStructureToResponse(SalaryStructure s)
        {
            var gross = s.BasicSalary
                      + s.HouseAllowance
                      + s.TransportAllowance
                      + s.MedicalAllowance
                      + s.OtherAllowances;

            return new SalaryStructureResponse
            {
                SalaryStructureId = s.SalaryStructureId,
                UserId = s.UserId,
                UserFullName = s.User?.FullName ?? string.Empty,
                UserEmail = s.User?.Email ?? string.Empty,
                BasicSalary = s.BasicSalary,
                HouseAllowance = s.HouseAllowance,
                TransportAllowance = s.TransportAllowance,
                MedicalAllowance = s.MedicalAllowance,
                OtherAllowances = s.OtherAllowances,
                GrossSalary = gross,
                TaxPercentage = s.TaxPercentage,
                PerDayRate = s.PerDayRate,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
        }

        // =============================================
        // PRIVATE: Map Payroll → Response
        // =============================================
        private PayrollResponse MapPayrollToResponse(Payroll p)
        {
            return new PayrollResponse
            {
                PayrollId = p.PayrollId,
                UserId = p.UserId,
                UserFullName = p.User?.FullName ?? string.Empty,
                UserEmail = p.User?.Email ?? string.Empty,
                Month = p.Month,
                Year = p.Year,
                BasicSalary = p.BasicSalary,
                HouseAllowance = p.HouseAllowance,
                TransportAllowance = p.TransportAllowance,
                MedicalAllowance = p.MedicalAllowance,
                OtherAllowances = p.OtherAllowances,
                GrossSalary = p.GrossSalary,
                WorkingDaysInMonth = p.WorkingDaysInMonth,
                PresentDays = p.PresentDays,
                AbsentDays = p.AbsentDays,
                LateDays = p.LateDays,
                HalfDays = p.HalfDays,
                TotalWorkingHours = p.TotalWorkingHours,
                PaidLeaveDays = p.PaidLeaveDays,
                UnpaidLeaveDays = p.UnpaidLeaveDays,
                AbsenceDeduction = p.AbsenceDeduction,
                LateDeduction = p.LateDeduction,
                HalfDayDeduction = p.HalfDayDeduction,
                UnpaidLeaveDeduction = p.UnpaidLeaveDeduction,
                TaxDeduction = p.TaxDeduction,
                OtherDeductions = p.OtherDeductions,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Status = p.Status.ToString(),
                ApprovedAt = p.ApprovedAt,
                PaidAt = p.PaidAt,
                PaymentNote = p.PaymentNote,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt
            };
        }
    }
}