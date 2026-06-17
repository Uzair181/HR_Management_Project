using HR_Management_System.DTOs;

namespace HR_Management_System.Interfaces
{
    public interface ILeaveService
    {
        // Employee
        Task<LeaveResponse> ApplyLeave(ApplyLeave dto);
        Task<List<LeaveResponse>> GetMyLeaves();
        Task<bool> CancelLeave(Guid leaveId);
        Task<LeaveResponse?> GetLeaveById(Guid leaveId);
        Task<LeaveBalanceResponse> GetMyLeaveBalance();

        // Admin + HR
        Task<List<LeaveResponse>> GetAllLeaves();
        Task<List<LeaveResponse>> GetLeavesByUser(Guid userId);
        Task<List<LeaveResponse>> GetPendingLeaves();
        Task<bool> ApproveLeave(Guid leaveId, LeaveAction dto);
        Task<bool> RejectLeave(Guid leaveId, LeaveAction dto);
        Task<LeaveBalanceResponse> GetLeaveBalanceByUser(Guid userId);
        Task<LeaveSummaryResponse> GetLeaveSummary();
        Task<List<LeaveReportResponse>> GetMonthlyReport(int month, int year);
        Task<List<LeaveReportResponse>> GetYearlyReport(int year);

        // Admin only
        Task<bool> DeleteLeave(Guid leaveId);
    }
}