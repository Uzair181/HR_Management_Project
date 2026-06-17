namespace HR_Management_System.Interfaces
{
    public interface IClaimsService
    {
        Guid GetUserId();
        Guid GetOrganizationId();
        string GetRole();
    }
}