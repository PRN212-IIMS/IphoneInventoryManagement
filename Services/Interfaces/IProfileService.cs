using Services.Models;

namespace Services.Interfaces
{
    public interface IProfileService
    {
        ProfileOperationResult UpdateProfile(UpdateProfileRequest request);
    }
}
