using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        // Annual leave allowances per year
        private const int AnnualAllowed = 20;
        private const int SickAllowed = 10;
        private const int CasualAllowed = 7;

        public LeaveService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =============================================
        // APPLY LEAVE
        // Employee submits leave request
        // Validates dates and no overlapping leaves
        // =============================================
        public async Task<LeaveResponse> ApplyLeave(ApplyLeave dto)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            // Validate leave type
            if (!Enum.TryParse<LeaveType>(dto.Type, true, out var leaveType))
                throw new Exception($"Invalid leave type. Valid types: {string.Join(", ", Enum.GetNames<LeaveType>())}");

            // Validate dates
            if (dto.FromDate.Date < DateTime.UtcNow.Date)
                throw new Exception("Leave cannot start in the past");

            if (dto.ToDate.Date < dto.FromDate.Date)
                throw new Exception("End date cannot be before start date");

            // Check overlapping leave
            var overlap = await _context.Leaves
                .AnyAsync(l => l.UserId == userId
                            && l.OrganizationId == orgId
                            && l.Status != LeaveStatus.Cancelled
                            && l.Status != LeaveStatus.Rejected
                            && l.FromDate <= dto.ToDate
                            && l.ToDate >= dto.FromDate);

            if (overlap)
                throw new Exception("You already have a leave request overlapping these dates");

            var totalDays = (dto.ToDate.Date - dto.FromDate.Date).Days + 1;

            var leave = new Leave
            {
                UserId = userId,
                OrganizationId = orgId,
                Type = leaveType,
                FromDate = dto.FromDate.Date,
                ToDate = dto.ToDate.Date,
                TotalDays = totalDays,
                Reason = dto.Reason,
                Status = LeaveStatus.Pending
            };

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            await _context.Entry(leave).Reference(l => l.User).LoadAsync();

            return MapToResponse(leave);
        }

        // =============================================
        // GET MY LEAVES
        // Employee sees only their own leave history
        // UserId always from JWT — never from request
        // =============================================
        public async Task<List<LeaveResponse>> GetMyLeaves()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var leaves = await _context.Leaves
                .Where(l => l.UserId == userId
                         && l.OrganizationId == orgId)
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return leaves.Select(MapToResponse).ToList();
        }

        // =============================================
        // CANCEL LEAVE
        // Employee cancels own pending leave only
        // Cannot cancel approved/rejected leaves
        // =============================================
        public async Task<bool> CancelLeave(Guid leaveId)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId
                                       && l.UserId == userId
                                       && l.OrganizationId == orgId);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != LeaveStatus.Pending)
                throw new Exception("Only pending leaves can be cancelled");

            leave.Status = LeaveStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // GET LEAVE BY ID
        // Employee can only see own leave
        // Admin/HR can see any leave in org
        // Role check handled in controller
        // =============================================
        public async Task<LeaveResponse?> GetLeaveById(Guid leaveId)
        {
            var orgId = _claims.GetOrganizationId();

            var leave = await _context.Leaves
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId
                                       && l.OrganizationId == orgId);

            if (leave == null) return null;

            return MapToResponse(leave);
        }

        // =============================================
        // GET MY LEAVE BALANCE
        // Employee checks their own remaining leaves
        // Calculated from approved leaves this year
        // =============================================
        public async Task<LeaveBalanceResponse> GetMyLeaveBalance()
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            return await CalculateBalance(userId, orgId);
        }

        // =============================================
        // GET ALL LEAVES
        // Admin + HR sees all org leave requests
        // =============================================
        public async Task<List<LeaveResponse>> GetAllLeaves()
        {
            var orgId = _claims.GetOrganizationId();

            var leaves = await _context.Leaves
                .Where(l => l.OrganizationId == orgId)
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return leaves.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET LEAVES BY USER
        // Admin + HR inspects specific employee leaves
        // Org check prevents cross-tenant access
        // =============================================
        public async Task<List<LeaveResponse>> GetLeavesByUser(Guid userId)
        {
            var orgId = _claims.GetOrganizationId();

            // Verify user belongs to same org
            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            var leaves = await _context.Leaves
                .Where(l => l.UserId == userId
                         && l.OrganizationId == orgId)
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return leaves.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET PENDING LEAVES
        // Admin + HR sees only pending requests
        // Useful for dashboard/action required view
        // =============================================
        public async Task<List<LeaveResponse>> GetPendingLeaves()
        {
            var orgId = _claims.GetOrganizationId();

            var leaves = await _context.Leaves
                .Where(l => l.OrganizationId == orgId
                         && l.Status == LeaveStatus.Pending)
                .Include(l => l.User)
                .OrderBy(l => l.FromDate)
                .ToListAsync();

            return leaves.Select(MapToResponse).ToList();
        }

        // =============================================
        // APPROVE LEAVE
        // Admin + HR approves a pending leave
        // Records who approved and when
        // =============================================
        public async Task<bool> ApproveLeave(Guid leaveId, LeaveAction dto)
        {
            var orgId = _claims.GetOrganizationId();
            var actionById = _claims.GetUserId();

            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId
                                       && l.OrganizationId == orgId);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != LeaveStatus.Pending)
                throw new Exception("Only pending leaves can be approved");

            leave.Status = LeaveStatus.Approved;
            leave.ActionBy = actionById;
            leave.ActionComment = dto.Comment;
            leave.ActionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // REJECT LEAVE
        // Admin + HR rejects a pending leave
        // Comment required to explain rejection
        // =============================================
        public async Task<bool> RejectLeave(Guid leaveId, LeaveAction dto)
        {
            var orgId = _claims.GetOrganizationId();
            var actionById = _claims.GetUserId();

            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId
                                       && l.OrganizationId == orgId);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != LeaveStatus.Pending)
                throw new Exception("Only pending leaves can be rejected");

            leave.Status = LeaveStatus.Rejected;
            leave.ActionBy = actionById;
            leave.ActionComment = dto.Comment;
            leave.ActionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // GET LEAVE BALANCE BY USER
        // Admin + HR checks specific employee balance
        // Org check prevents cross-tenant access
        // =============================================
        public async Task<LeaveBalanceResponse> GetLeaveBalanceByUser(Guid userId)
        {
            var orgId = _claims.GetOrganizationId();

            var userExists = await _context.Users
                .AnyAsync(u => u.UserId == userId
                            && u.OrganizationId == orgId);

            if (!userExists)
                throw new Exception("User not found in your organization");

            return await CalculateBalance(userId, orgId);
        }

        // =============================================
        // GET LEAVE SUMMARY
        // Admin + HR dashboard overview
        // Shows counts by status + recent leaves
        // =============================================
        public async Task<LeaveSummaryResponse> GetLeaveSummary()
        {
            var orgId = _claims.GetOrganizationId();

            var allLeaves = await _context.Leaves
                .Where(l => l.OrganizationId == orgId)
                .Include(l => l.User)
                .ToListAsync();

            var recent = allLeaves
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .Select(MapToResponse)
                .ToList();

            return new LeaveSummaryResponse
            {
                TotalPending = allLeaves.Count(l => l.Status == LeaveStatus.Pending),
                TotalApproved = allLeaves.Count(l => l.Status == LeaveStatus.Approved),
                TotalRejected = allLeaves.Count(l => l.Status == LeaveStatus.Rejected),
                TotalCancelled = allLeaves.Count(l => l.Status == LeaveStatus.Cancelled),
                RecentLeaves = recent
            };
        }

        // =============================================
        // MONTHLY REPORT
        // Admin + HR — per user leave stats for a month
        // =============================================
        public async Task<List<LeaveReportResponse>> GetMonthlyReport(int month, int year)
        {
            var orgId = _claims.GetOrganizationId();

            var leaves = await _context.Leaves
                .Where(l => l.OrganizationId == orgId
                         && l.FromDate.Month == month
                         && l.FromDate.Year == year)
                .Include(l => l.User)
                .ToListAsync();

            return leaves
                .GroupBy(l => l.User)
                .Select(g => new LeaveReportResponse
                {
                    UserId = g.Key.UserId,
                    UserFullName = g.Key.FullName,
                    UserEmail = g.Key.Email,
                    TotalLeaves = g.Count(),
                    ApprovedLeaves = g.Count(l => l.Status == LeaveStatus.Approved),
                    PendingLeaves = g.Count(l => l.Status == LeaveStatus.Pending),
                    RejectedLeaves = g.Count(l => l.Status == LeaveStatus.Rejected),
                    TotalDaysTaken = g.Where(l => l.Status == LeaveStatus.Approved)
                                       .Sum(l => l.TotalDays)
                })
                .ToList();
        }

        // =============================================
        // YEARLY REPORT
        // Admin + HR — full year leave stats per user
        // =============================================
        public async Task<List<LeaveReportResponse>> GetYearlyReport(int year)
        {
            var orgId = _claims.GetOrganizationId();

            var leaves = await _context.Leaves
                .Where(l => l.OrganizationId == orgId
                         && l.FromDate.Year == year)
                .Include(l => l.User)
                .ToListAsync();

            return leaves
                .GroupBy(l => l.User)
                .Select(g => new LeaveReportResponse
                {
                    UserId = g.Key.UserId,
                    UserFullName = g.Key.FullName,
                    UserEmail = g.Key.Email,
                    TotalLeaves = g.Count(),
                    ApprovedLeaves = g.Count(l => l.Status == LeaveStatus.Approved),
                    PendingLeaves = g.Count(l => l.Status == LeaveStatus.Pending),
                    RejectedLeaves = g.Count(l => l.Status == LeaveStatus.Rejected),
                    TotalDaysTaken = g.Where(l => l.Status == LeaveStatus.Approved)
                                      .Sum(l => l.TotalDays)
                })
                .ToList();
        }

        // =============================================
        // DELETE LEAVE
        // Admin only — permanent removal
        // =============================================
        public async Task<bool> DeleteLeave(Guid leaveId)
        {
            var orgId = _claims.GetOrganizationId();

            var leave = await _context.Leaves
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId
                                       && l.OrganizationId == orgId);

            if (leave == null) return false;

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // PRIVATE: Calculate Leave Balance
        // Counts approved leaves this year by type
        // Used by both GetMyLeaveBalance and
        // GetLeaveBalanceByUser
        // =============================================
        private async Task<LeaveBalanceResponse> CalculateBalance(Guid userId, Guid orgId)
        {
            var currentYear = DateTime.UtcNow.Year;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var approvedLeaves = await _context.Leaves
                .Where(l => l.UserId == userId
                         && l.OrganizationId == orgId
                         && l.Status == LeaveStatus.Approved
                         && l.FromDate.Year == currentYear)
                .ToListAsync();

            var annualUsed = approvedLeaves
                .Where(l => l.Type == LeaveType.Annual)
                .Sum(l => l.TotalDays);

            var sickUsed = approvedLeaves
                .Where(l => l.Type == LeaveType.Sick)
                .Sum(l => l.TotalDays);

            var casualUsed = approvedLeaves
                .Where(l => l.Type == LeaveType.Casual)
                .Sum(l => l.TotalDays);

            return new LeaveBalanceResponse
            {
                UserId = userId,
                UserFullName = user?.FullName ?? string.Empty,
                AnnualAllowed = AnnualAllowed,
                SickAllowed = SickAllowed,
                CasualAllowed = CasualAllowed,
                AnnualUsed = annualUsed,
                SickUsed = sickUsed,
                CasualUsed = casualUsed,
                AnnualRemaining = AnnualAllowed - annualUsed,
                SickRemaining = SickAllowed - sickUsed,
                CasualRemaining = CasualAllowed - casualUsed
            };
        }

        // =============================================
        // PRIVATE: Map Leave → LeaveResponse
        // Never expose raw model to API response
        // =============================================
        private LeaveResponse MapToResponse(Leave leave)
        {
            return new LeaveResponse
            {
                LeaveId = leave.LeaveId,
                UserId = leave.UserId,
                UserFullName = leave.User?.FullName ?? string.Empty,
                UserEmail = leave.User?.Email ?? string.Empty,
                Type = leave.Type.ToString(),
                FromDate = leave.FromDate,
                ToDate = leave.ToDate,
                TotalDays = leave.TotalDays,
                Reason = leave.Reason,
                Status = leave.Status.ToString(),
                ActionComment = leave.ActionComment,
                ActionDate = leave.ActionDate,
                CreatedAt = leave.CreatedAt
            };
        }
    }
}