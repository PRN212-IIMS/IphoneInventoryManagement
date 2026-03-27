using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using BusinessObjects;
using DataAccessLayer;
using Microsoft.Extensions.Configuration;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Helpers;
using Services.Interfaces;
using Services.Models;

namespace Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ICustomerRepository _customerRepository = new CustomerRepository();

    public AuthenticatedUser? Login(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedPassword = password.Trim();

        using var context = new IPhoneInventoryDbContext();

        var admin = context.Admins.FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail && x.Status == "Active");
        if (admin is not null && IsValidHashedPassword(admin.Password, normalizedPassword))
        {
            return new AuthenticatedUser
            {
                UserId = admin.AdminId,
                Role = "Admin",
                FullName = admin.FullName,
                Email = admin.Email,
                Status = admin.Status
            };
        }

        var defaultAdmin = LoadDefaultAdmin();
        if (defaultAdmin is not null
            && string.Equals(defaultAdmin.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && defaultAdmin.Password == normalizedPassword)
        {
            return new AuthenticatedUser
            {
                Role = "Admin",
                FullName = "System Admin",
                Email = normalizedEmail,
                Status = "Active"
            };
        }

        var staff = context.Staff.FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail && x.Status == "Active");
        if (staff is not null && IsValidHashedPassword(staff.Password, normalizedPassword))
        {
            return new AuthenticatedUser
            {
                UserId = staff.StaffId,
                Role = "Staff",
                FullName = staff.FullName,
                Email = staff.Email,
                Phone = staff.Phone ?? string.Empty,
                Status = staff.Status
            };
        }

        var customer = context.Customers.FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail && x.Status == "Active");
        if (customer is not null && IsValidHashedPassword(customer.Password, normalizedPassword))
        {
            return new AuthenticatedUser
            {
                UserId = customer.CustomerId,
                Role = "Customer",
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone ?? string.Empty,
                Status = customer.Status
            };
        }

        return null;
    }

    public RegisterCustomerResult RegisterCustomer(RegisterCustomerRequest request)
    {
        var fullName = request.FullName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();
        var password = request.Password.Trim();
        var confirmPassword = request.ConfirmPassword.Trim();

        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return new RegisterCustomerResult { Success = false, Message = "Please fill in all registration fields." };
        }

        if (!IsValidFullName(fullName))
        {
            return new RegisterCustomerResult { Success = false, Message = "Full name must contain letters only." };
        }

        if (!IsValidEmail(email))
        {
            return new RegisterCustomerResult { Success = false, Message = "Please enter a valid email address." };
        }

        if (!IsValidPhone(phone))
        {
            return new RegisterCustomerResult { Success = false, Message = "Phone number must be 10 digits and start with 0." };
        }

        if (!IsValidPassword(password))
        {
            return new RegisterCustomerResult { Success = false, Message = "Password must contain at least 1 uppercase letter, 1 number, 1 special character, and no spaces." };
        }

        if (password != confirmPassword)
        {
            return new RegisterCustomerResult { Success = false, Message = "Password confirmation does not match." };
        }

        using var context = new IPhoneInventoryDbContext();
        var emailExists = context.Admins.Any(x => x.Email != null && x.Email.ToLower() == email)
            || context.Staff.Any(x => x.Email != null && x.Email.ToLower() == email)
            || context.Customers.Any(x => x.Email != null && x.Email.ToLower() == email);

        if (emailExists)
        {
            return new RegisterCustomerResult { Success = false, Message = "This email is already in use." };
        }

        var customer = new Customer
        {
            FullName = fullName,
            Email = email,
            Phone = phone,
            Password = PasswordHasher.Hash(password),
            Status = "Active"
        };

        try
        {
            _customerRepository.AddCustomer(customer);
            return new RegisterCustomerResult { Success = true, Message = "Account created successfully. Please sign in." };
        }
        catch
        {
            return new RegisterCustomerResult { Success = false, Message = "Unable to create the account right now." };
        }
    }

    private static bool IsValidHashedPassword(string storedPassword, string providedPassword)
    {
        return PasswordHasher.IsHash(storedPassword) && PasswordHasher.Verify(providedPassword, storedPassword);
    }

    private static bool IsValidFullName(string fullName)
    {
        return Regex.IsMatch(fullName, @"^[\p{L} ]+$");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^0\d{9}$");
    }

    private static bool IsValidPassword(string password)
    {
        if (password.Length < 8 || password.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var hasUppercase = password.Any(char.IsUpper);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialCharacter = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUppercase && hasDigit && hasSpecialCharacter;
    }

    private static DefaultAdminAccount? LoadDefaultAdmin()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var email = configuration["DefaultAdminAccount:Email"];
        var password = configuration["DefaultAdminAccount:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        return new DefaultAdminAccount
        {
            Email = email,
            Password = password
        };
    }

    private sealed class DefaultAdminAccount
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
