using HR_Management_System.DTOs;

namespace HR_Management_System.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResponse> GetMyProfile();
        Task<ProfileResponse> UpdateMyProfile(UpdateProfile dto);
        Task ChangePassword(ChangePassword dto);
    }
}