using System.Net.Mail;
using System.Text.RegularExpressions;
using BusinessObjects;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implementations;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffService()
    {
        _staffRepository = new StaffRepository();
    }

    public List<Staff> GetAllStaff()
    {
        return _staffRepository.GetAllStaff();
    }

    public Staff? GetStaffById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return _staffRepository.GetStaffById(id);
    }

    public List<Staff> SearchStaff(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return _staffRepository.GetAllStaff();
        }

        return _staffRepository.SearchStaff(keyword);
    }

    public void CreateStaff(Staff staff)
    {
        if (staff is null)
        {
            throw new ArgumentNullException(nameof(staff));
        }

        var fullName = staff.FullName?.Trim() ?? string.Empty;
        var email = staff.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var phone = staff.Phone?.Trim() ?? string.Empty;
        var password = staff.Password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new Exception("Please fill in all staff fields.");
        }

        if (!Regex.IsMatch(fullName, @"^[\p{L} ]+$"))
        {
            throw new Exception("Full name must contain letters only.");
        }

        if (!IsValidEmail(email))
        {
            throw new Exception("Email is not valid.");
        }

        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
        {
            throw new Exception("Phone number must be 10 digits and start with 0.");
        }

        if (!IsValidPassword(password))
        {
            throw new Exception("Password must be at least 8 characters, include 1 uppercase letter, 1 number, 1 special character, and contain no spaces.");
        }

        var existing = _staffRepository.GetStaffByEmail(email);
        if (existing is not null)
        {
            throw new Exception("Email is already used by another staff account.");
        }

        staff.FullName = fullName;
        staff.Email = email;
        staff.Phone = phone;
        staff.Password = PasswordHasher.Hash(password);
        staff.Status = string.IsNullOrWhiteSpace(staff.Status) ? "Active" : staff.Status;

        _staffRepository.AddStaff(staff);
    }

    public void UpdateStaff(Staff staff)
    {
        if (staff is null)
        {
            throw new ArgumentNullException(nameof(staff));
        }

        if (staff.StaffId <= 0)
        {
            throw new Exception("Staff ID is invalid.");
        }

        var existing = _staffRepository.GetStaffById(staff.StaffId);
        if (existing is null)
        {
            throw new Exception("Staff not found.");
        }

        var fullName = staff.FullName?.Trim() ?? string.Empty;
        var phone = staff.Phone?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
        {
            throw new Exception("Full name and phone must not be empty.");
        }

        if (!Regex.IsMatch(fullName, @"^[\p{L} ]+$"))
        {
            throw new Exception("Full name must contain letters only.");
        }

        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
        {
            throw new Exception("Phone number must be 10 digits and start with 0.");
        }

        existing.FullName = fullName;
        existing.Phone = phone;
        existing.Status = string.IsNullOrWhiteSpace(staff.Status) ? existing.Status : staff.Status;
        _staffRepository.UpdateStaff(existing);
    }

    public void ChangeStaffStatus(int staffId, string status)
    {
        if (staffId <= 0)
        {
            throw new Exception("Staff ID is invalid.");
        }

        var normalizedStatus = status.Trim();
        if (normalizedStatus != "Active" && normalizedStatus != "Inactive")
        {
            throw new Exception("Status must be Active or Inactive.");
        }

        _staffRepository.UpdateStatus(staffId, normalizedStatus);
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
}
