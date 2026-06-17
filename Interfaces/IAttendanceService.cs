using HR_Management_System.DTOs;

namespace HR_Management_System.Interfaces
{
    public interface IAttendanceService
    {
        // Employee
        Task<AttendanceResponse> CheckIn(CheckInRequest request);
        Task<AttendanceResponse> CheckOut(CheckOutRequest request);
        Task<List<AttendanceResponse>> GetMyAttendance();
        Task<AttendanceResponse?> GetMyStatusByDate(DateTime date);

        // Admin + HR
        Task<List<AttendanceResponse>> GetAllAttendance();
        Task<List<AttendanceResponse>> GetUserAttendance(Guid userId);
        Task<AttendanceResponse> AddManualAttendance(ManualAttendanceRequest request);
        Task<AttendanceResponse?> UpdateAttendance(Guid id, UpdateAttendanceRequest request);

        // Admin only
        Task<bool> DeleteAttendance(Guid id);

        // Reports
        Task<List<MonthlyReportResponse>> GetMonthlyReport(int month, int year);
        Task<DailySummaryResponse> GetDailySummary(DateTime date);
    }
}