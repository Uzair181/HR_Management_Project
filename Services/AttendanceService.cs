using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        // Office start time — Late if CheckIn after this
        private readonly TimeSpan _officeStartTime = new TimeSpan(9, 0, 0); // 9:00 AM

        // HalfDay threshold
        private readonly double _halfDayHours = 4.0;

        public AttendanceService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =============================================
        // CHECK IN
        // Employee marks arrival
        // Only once per day allowed
        // =============================================
        public async Task<AttendanceResponse> CheckIn(CheckInRequest request)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();
            var today = DateTime.UtcNow.Date;

            // Prevent double check-in
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.Date == today);

            if (existing != null)
                throw new Exception("You have already checked in today");

            var now = DateTime.UtcNow;

            // Auto-determine status based on check-in time
            var status = now.TimeOfDay <= _officeStartTime
                ? AttendanceStatus.Present
                : AttendanceStatus.Late;

            var attendance = new Attendance
            {
                UserId = userId,
                OrganizationId = orgId,
                Date = today,
                CheckIn = now,
                Status = status,
                Notes = request.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return await MapToResponse(attendance);
        }

        // =============================================
        // CHECK OUT
        // Employee marks departure
        // Must have checked in first
        // Auto-calculates WorkingHours and updates Status
        // =============================================
        public async Task<AttendanceResponse> CheckOut(CheckOutRequest request)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();
            var today = DateTime.UtcNow.Date;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.Date == today
                                       && a.OrganizationId == orgId);

            if (attendance == null)
                throw new Exception("You have not checked in today");

            if (attendance.CheckOut != null)
                throw new Exception("You have already checked out today");

            var now = DateTime.UtcNow;
            attendance.CheckOut = now;

            // Calculate working hours
            attendance.WorkingHours = (now - attendance.CheckIn!.Value).TotalHours;

            // Update status based on working hours
            if (attendance.WorkingHours < _halfDayHours)
                attendance.Status = AttendanceStatus.HalfDay;

            if (request.Notes != null)
                attendance.Notes = request.Notes;

            await _context.SaveChangesAsync();

            return await MapToResponse(attendance);
        }

        // =============================================
        // GET MY ATTENDANCE
        // Employee sees only their own records
        // =============================================
        public async Task<List<AttendanceResponse>> GetMyAttendance()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var records = await _context.Attendances
                .Where(a => a.UserId == userId
                         && a.OrganizationId == orgId)
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return records.Select(a => MapToResponseSync(a)).ToList();
        }

        // =============================================
        // GET MY STATUS BY DATE
        // Employee checks attendance for a specific date
        // =============================================
        public async Task<AttendanceResponse?> GetMyStatusByDate(DateTime date)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var record = await _context.Attendances
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.OrganizationId == orgId
                                       && a.Date == date.Date);

            if (record == null) return null;

            return MapToResponseSync(record);
        }

        // =============================================
        // GET ALL ATTENDANCE
        // Admin + HR sees entire organization attendance
        // =============================================
        public async Task<List<AttendanceResponse>> GetAllAttendance()
        {
            var orgId = _claims.GetOrganizationId();

            var records = await _context.Attendances
                .Where(a => a.OrganizationId == orgId)
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return records.Select(a => MapToResponseSync(a)).ToList();
        }

        // =============================================
        // GET USER ATTENDANCE
        // Admin + HR views specific employee attendance
        // Org check prevents cross-tenant access
        // =============================================
        public async Task<List<AttendanceResponse>> GetUserAttendance(Guid userId)
        {
            var orgId = _claims.GetOrganizationId();

            // Verify user belongs to same org
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            var records = await _context.Attendances
                .Where(a => a.UserId == userId
                         && a.OrganizationId == orgId)
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return records.Select(a => MapToResponseSync(a)).ToList();
        }

        // =============================================
        // ADD MANUAL ATTENDANCE
        // Admin + HR adds attendance for any employee
        // Used for corrections or missed records
        // =============================================
        public async Task<AttendanceResponse> AddManualAttendance(ManualAttendanceRequest request)
        {
            var orgId = _claims.GetOrganizationId();

            // Verify target user is in same org
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == request.UserId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            // Prevent duplicate record for same day
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == request.UserId
                                       && a.Date == request.Date.Date);

            if (existing != null)
                throw new Exception("Attendance record already exists for this date");

            double workingHours = 0;
            var status = AttendanceStatus.Absent;

            if (request.CheckIn != null)
            {
                status = request.CheckIn.Value.TimeOfDay <= _officeStartTime
                    ? AttendanceStatus.Present
                    : AttendanceStatus.Late;

                if (request.CheckOut != null)
                {
                    workingHours = (request.CheckOut.Value - request.CheckIn.Value).TotalHours;

                    if (workingHours < _halfDayHours)
                        status = AttendanceStatus.HalfDay;
                }
            }

            var attendance = new Attendance
            {
                UserId = request.UserId,
                OrganizationId = orgId,
                Date = request.Date.Date,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                WorkingHours = workingHours,
                Status = status,
                Notes = request.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            await _context.Entry(attendance).Reference(a => a.User).LoadAsync();

            return MapToResponseSync(attendance);
        }

        // =============================================
        // UPDATE ATTENDANCE
        // Admin + HR corrects an existing record
        // Recalculates WorkingHours and Status on update
        // =============================================
        public async Task<AttendanceResponse?> UpdateAttendance(Guid id, UpdateAttendanceRequest request)
        {
            var orgId = _claims.GetOrganizationId();

            var attendance = await _context.Attendances
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AttendanceId == id
                                       && a.OrganizationId == orgId);

            if (attendance == null) return null;

            if (request.CheckIn != null) attendance.CheckIn = request.CheckIn;
            if (request.CheckOut != null) attendance.CheckOut = request.CheckOut;
            if (request.Notes != null) attendance.Notes = request.Notes;

            // Recalculate working hours and status
            if (attendance.CheckIn != null)
            {
                attendance.Status = attendance.CheckIn.Value.TimeOfDay <= _officeStartTime
                    ? AttendanceStatus.Present
                    : AttendanceStatus.Late;

                if (attendance.CheckOut != null)
                {
                    attendance.WorkingHours =
                        (attendance.CheckOut.Value - attendance.CheckIn.Value).TotalHours;

                    if (attendance.WorkingHours < _halfDayHours)
                        attendance.Status = AttendanceStatus.HalfDay;
                }
            }

            await _context.SaveChangesAsync();

            return MapToResponseSync(attendance);
        }

        // =============================================
        // DELETE ATTENDANCE
        // Admin only — permanent removal
        // Org check prevents cross-tenant deletion
        // =============================================
        public async Task<bool> DeleteAttendance(Guid id)
        {
            var orgId = _claims.GetOrganizationId();

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.AttendanceId == id
                                       && a.OrganizationId == orgId);

            if (attendance == null) return false;

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // MONTHLY REPORT
        // Admin + HR gets per-user summary for a month
        // Shows Present/Late/HalfDay/Absent counts
        // =============================================
        public async Task<List<MonthlyReportResponse>> GetMonthlyReport(int month, int year)
        {
            var orgId = _claims.GetOrganizationId();

            var records = await _context.Attendances
                .Where(a => a.OrganizationId == orgId
                         && a.Date.Month == month
                         && a.Date.Year == year)
                .Include(a => a.User)
                .ToListAsync();

            // Group by user and calculate stats
            var report = records
                .GroupBy(a => a.User)
                .Select(g => new MonthlyReportResponse
                {
                    UserId = g.Key.UserId,
                    UserFullName = g.Key.FullName,
                    UserEmail = g.Key.Email,
                    Month = month,
                    Year = year,
                    TotalDays = g.Count(),
                    PresentDays = g.Count(a => a.Status == AttendanceStatus.Present),
                    LateDays = g.Count(a => a.Status == AttendanceStatus.Late),
                    HalfDays = g.Count(a => a.Status == AttendanceStatus.HalfDay),
                    AbsentDays = g.Count(a => a.Status == AttendanceStatus.Absent),
                    TotalWorkingHours = g.Sum(a => a.WorkingHours)
                })
                .ToList();

            return report;
        }

        // =============================================
        // DAILY SUMMARY
        // Admin + HR gets full org snapshot for one day
        // Shows counts + all individual records
        // =============================================
        public async Task<DailySummaryResponse> GetDailySummary(DateTime date)
        {
            var orgId = _claims.GetOrganizationId();

            var records = await _context.Attendances
                .Where(a => a.OrganizationId == orgId
                         && a.Date == date.Date)
                .Include(a => a.User)
                .ToListAsync();

            // Total users in org for absent calculation
            var totalUsers = await _context.Users
                .CountAsync(u => u.OrganizationId == orgId);

            return new DailySummaryResponse
            {
                Date = date.Date,
                TotalEmployees = totalUsers,
                Present = records.Count(a => a.Status == AttendanceStatus.Present),
                Late = records.Count(a => a.Status == AttendanceStatus.Late),
                HalfDay = records.Count(a => a.Status == AttendanceStatus.HalfDay),
                Absent = totalUsers - records.Count, // those with no record
                Records = records.Select(a => MapToResponseSync(a)).ToList()
            };
        }

        // =============================================
        // PRIVATE: Map Attendance → AttendanceResponse
        // Never expose raw model to API
        // =============================================
        private AttendanceResponse MapToResponseSync(Attendance a)
        {
            return new AttendanceResponse
            {
                AttendanceId = a.AttendanceId,
                UserId = a.UserId,
                UserFullName = a.User?.FullName ?? string.Empty,
                UserEmail = a.User?.Email ?? string.Empty,
                Date = a.Date,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                WorkingHours = a.WorkingHours,
                Status = a.Status.ToString(),
                Notes = a.Notes
            };
        }

        private async Task<AttendanceResponse> MapToResponse(Attendance a)
        {
            await _context.Entry(a).Reference(x => x.User).LoadAsync();
            return MapToResponseSync(a);
        }
    }
}