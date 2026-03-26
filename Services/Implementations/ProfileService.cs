using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Models;

namespace Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly ICustomerRepository _customerRepository = new CustomerRepository();
    private readonly IStaffRepository _staffRepository = new StaffRepository();

    public ProfileOperationResult UpdateProfile(UpdateProfileRequest request)
    {
        var fullName = request.FullName.Trim();
        var phone = request.Phone.Trim();

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
        {
            return new ProfileOperationResult { Success = false, Message = "Please fill in full name and phone number." };
        }

        if (!Regex.IsMatch(fullName, @"^[\p{L} ]+$"))
        {
            return new ProfileOperationResult { Success = false, Message = "Full name must contain letters only." };
        }

        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
        {
            return new ProfileOperationResult { Success = false, Message = "Phone number must be 10 digits and start with 0." };
        }

        if (request.Role == "Customer")
        {
            var customer = _customerRepository.GetCustomerById(request.UserId);
            if (customer is null)
            {
                return new ProfileOperationResult { Success = false, Message = "Customer account was not found." };
            }

            customer.FullName = fullName;
            customer.Phone = phone;
            _customerRepository.UpdateCustomer(customer);

            return new ProfileOperationResult
            {
                Success = true,
                Message = "Profile updated successfully.",
                UpdatedUser = new AuthenticatedUser
                {
                    UserId = customer.CustomerId,
                    Role = "Customer",
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone ?? string.Empty,
                    Status = customer.Status
                }
            };
        }

        if (request.Role == "Staff")
        {
            var staff = _staffRepository.GetStaffById(request.UserId);
            if (staff is null)
            {
                return new ProfileOperationResult { Success = false, Message = "Staff account was not found." };
            }

            staff.FullName = fullName;
            staff.Phone = phone;
            _staffRepository.UpdateStaff(staff);

            return new ProfileOperationResult
            {
                Success = true,
                Message = "Profile updated successfully.",
                UpdatedUser = new AuthenticatedUser
                {
                    UserId = staff.StaffId,
                    Role = "Staff",
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Phone = staff.Phone ?? string.Empty,
                    Status = staff.Status
                }
            };
        }

        return new ProfileOperationResult { Success = false, Message = "This account cannot edit profile information." };
    }
}
