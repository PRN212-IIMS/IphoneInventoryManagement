using Services.Models;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        AuthenticatedUser? Login(string email, string password);
        RegisterCustomerResult RegisterCustomer(RegisterCustomerRequest request);
    }
}
