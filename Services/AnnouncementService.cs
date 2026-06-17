using HR_Management_System.Data;
using HR_Management_System.DTOs;
using HR_Management_System.Interfaces;
using HR_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Management_System.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly HRDbContext _context;
        private readonly IClaimsService _claims;

        public AnnouncementService(HRDbContext context, IClaimsService claims)
        {
            _context = context;
            _claims = claims;
        }

        // =============================================
        // CREATE ANNOUNCEMENT
        // Admin + HR creates org-wide announcement
        // Target controls who sees it
        // =============================================
        public async Task<AnnouncementResponse> CreateAnnouncement(CreateAnnouncement dto)
        {
            var userId = _claims.GetUserId();
            var orgId = _claims.GetOrganizationId();

            if (!Enum.TryParse<AnnouncementPriority>(dto.Priority, true, out var priority))
                throw new Exception($"Invalid priority. Valid: {string.Join(", ", Enum.GetNames<AnnouncementPriority>())}");

            if (!Enum.TryParse<AnnouncementTarget>(dto.Target, true, out var target))
                throw new Exception($"Invalid target. Valid: {string.Join(", ", Enum.GetNames<AnnouncementTarget>())}");

            var announcement = new Announcement
            {
                OrganizationId = orgId,
                CreatedByUserId = userId,
                Title = dto.Title,
                Content = dto.Content,
                Priority = priority,
                Target = target,
                ExpiresAt = dto.ExpiresAt,
                IsActive = true
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            await _context.Entry(announcement)
                .Reference(a => a.CreatedBy).LoadAsync();

            return MapToResponse(announcement);
        }

        // =============================================
        // UPDATE ANNOUNCEMENT
        // Admin + HR updates own announcement
        // =============================================
        public async Task<AnnouncementResponse?> UpdateAnnouncement(
            Guid id, UpdateAnnouncementDto dto)
        {
            var orgId = _claims.GetOrganizationId();

            var announcement = await _context.Announcements
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id
                                       && a.OrganizationId == orgId);

            if (announcement == null) return null;

            if (!Enum.TryParse<AnnouncementPriority>(dto.Priority, true, out var priority))
                throw new Exception($"Invalid priority. Valid: {string.Join(", ", Enum.GetNames<AnnouncementPriority>())}");

            if (!Enum.TryParse<AnnouncementTarget>(dto.Target, true, out var target))
                throw new Exception($"Invalid target. Valid: {string.Join(", ", Enum.GetNames<AnnouncementTarget>())}");

            announcement.Title = dto.Title;
            announcement.Content = dto.Content;
            announcement.Priority = priority;
            announcement.Target = target;
            announcement.ExpiresAt = dto.ExpiresAt;
            announcement.IsActive = dto.IsActive;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(announcement);
        }

        // =============================================
        // GET ALL ANNOUNCEMENTS
        // Admin + HR sees everything including inactive
        // =============================================
        public async Task<List<AnnouncementResponse>> GetAllAnnouncements()
        {
            var orgId = _claims.GetOrganizationId();

            var announcements = await _context.Announcements
                .Where(a => a.OrganizationId == orgId)
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return announcements.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET MY ANNOUNCEMENTS
        // Filtered by role + active + not expired
        // Employee sees All + Employee targeted
        // HR sees All + HR targeted
        // =============================================
        public async Task<List<AnnouncementResponse>> GetMyAnnouncements()
        {
            var orgId = _claims.GetOrganizationId();
            var role = _claims.GetRole();
            var now = DateTime.UtcNow;

            var query = _context.Announcements
                .Where(a => a.OrganizationId == orgId
                         && a.IsActive
                         && (a.ExpiresAt == null || a.ExpiresAt > now));

            // Filter by target based on role
            if (role == "Employee")
            {
                query = query.Where(a =>
                    a.Target == AnnouncementTarget.All ||
                    a.Target == AnnouncementTarget.Employee);
            }
            else if (role == "HR")
            {
                query = query.Where(a =>
                    a.Target == AnnouncementTarget.All ||
                    a.Target == AnnouncementTarget.HR);
            }
            // Admin sees everything

            var announcements = await query
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return announcements.Select(MapToResponse).ToList();
        }

        // =============================================
        // GET BY ID
        // Org check always applied
        // =============================================
        public async Task<AnnouncementResponse?> GetAnnouncementById(Guid id)
        {
            var orgId = _claims.GetOrganizationId();

            var announcement = await _context.Announcements
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id
                                       && a.OrganizationId == orgId);

            if (announcement == null) return null;

            return MapToResponse(announcement);
        }

        // =============================================
        // TOGGLE ACTIVE
        // Admin activates or deactivates announcement
        // =============================================
        public async Task<bool> ToggleActive(Guid id)
        {
            var orgId = _claims.GetOrganizationId();

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id
                                       && a.OrganizationId == orgId);

            if (announcement == null) return false;

            announcement.IsActive = !announcement.IsActive;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // DELETE ANNOUNCEMENT
        // Admin only — permanent removal
        // =============================================
        public async Task<bool> DeleteAnnouncement(Guid id)
        {
            var orgId = _claims.GetOrganizationId();

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id
                                       && a.OrganizationId == orgId);

            if (announcement == null) return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        // =============================================
        // PRIVATE: Map → Response
        // =============================================
        private AnnouncementResponse MapToResponse(Announcement a)
        {
            return new AnnouncementResponse
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                Priority = a.Priority.ToString(),
                Target = a.Target.ToString(),
                CreatedByName = a.CreatedBy?.FullName ?? string.Empty,
                ExpiresAt = a.ExpiresAt,
                IsActive = a.IsActive,
                IsExpired = a.ExpiresAt.HasValue && a.ExpiresAt < DateTime.UtcNow,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };
        }
    }
}