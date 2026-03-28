using Services.Models;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        LoginResult Login(string email, string password);
        RegisterCustomerResult RegisterCustomer(RegisterCustomerRequest request);
    }
}
