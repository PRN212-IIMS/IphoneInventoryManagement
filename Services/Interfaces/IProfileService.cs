using Services.Models;

namespace Services.Interfaces
{
    public interface IProfileService
    {
        ProfileOperationResult UpdateProfile(UpdateProfileRequest request);
        ProfileOperationResult ChangePassword(UpdatePasswordRequest request);
    }
}
