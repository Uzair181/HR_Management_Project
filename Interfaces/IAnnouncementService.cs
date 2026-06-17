using HR_Management_System.DTOs;

namespace HR_Management_System.Interfaces
{
    public interface IAnnouncementService
    {
        // Admin + HR
        Task<AnnouncementResponse> CreateAnnouncement(CreateAnnouncement dto);
        Task<AnnouncementResponse?> UpdateAnnouncement(Guid id, UpdateAnnouncementDto dto);
        Task<List<AnnouncementResponse>> GetAllAnnouncements();

        // Admin only
        Task<bool> DeleteAnnouncement(Guid id);
        Task<bool> ToggleActive(Guid id);

        // All roles — filtered by target role
        Task<List<AnnouncementResponse>> GetMyAnnouncements();
        Task<AnnouncementResponse?> GetAnnouncementById(Guid id);
    }
}