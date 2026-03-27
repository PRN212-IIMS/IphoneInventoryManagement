using BusinessObjects;
using DataAccessLayer;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Services.Helpers;

namespace Services.Implementations
{
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

        public List<Staff> FilterStaff(string? keyword, string? status)
        {
            return _staffRepository.FilterStaff(keyword, status);
        }

        public bool CreateStaff(Staff staff, out string message)
        {
            if (staff == null)
            {
                message = "Staff data is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(staff.FullName) ||
                string.IsNullOrWhiteSpace(staff.Email) ||
                string.IsNullOrWhiteSpace(staff.Password))
            {
                message = "Full name, email, and password are required.";
                return false;
            }

            if (!IsValidFullName(staff.FullName.Trim()))
            {
                message = "Full name must contain letters only.";
                return false;
            }

            if (!IsValidEmail(staff.Email.Trim()))
            {
                message = "Please enter a valid email address.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(staff.Phone) && !IsValidPhone(staff.Phone.Trim()))
            {
                message = "Phone number must be 10 digits and start with 0.";
                return false;
            }

            if (!IsValidPassword(staff.Password.Trim()))
            {
                message = "Password must contain at least 8 characters, 1 uppercase letter, 1 number, 1 special character, and no spaces.";
                return false;
            }

            using var context = new IPhoneInventoryDbContext();
            string normalizedEmail = staff.Email.Trim().ToLowerInvariant();

            bool emailExists =
                context.Admins.Any(x => x.Email != null && x.Email.ToLower() == normalizedEmail) ||
                context.Staff.Any(x => x.Email != null && x.Email.ToLower() == normalizedEmail) ||
                context.Customers.Any(x => x.Email != null && x.Email.ToLower() == normalizedEmail);

            if (emailExists)
            {
                message = "This email is already in use.";
                return false;
            }

            staff.FullName = staff.FullName.Trim();
            staff.Email = normalizedEmail;
            string plainPassword = staff.Password.Trim();
            staff.Password = PasswordHasher.Hash(plainPassword);
            staff.Phone = string.IsNullOrWhiteSpace(staff.Phone) ? null : staff.Phone.Trim();
            staff.Status = NormalizeStatus(staff.Status);

            try
            {
                _staffRepository.AddStaff(staff);
                message = "Staff account created successfully.";
                return true;
            }
            catch
            {
                message = "Unable to create staff account.";
                return false;
            }
        }

        public bool UpdateStaff(Staff staff, out string message)
        {
            var existingStaff = _staffRepository.GetStaffById(staff.StaffId);
            if (existingStaff == null)
            {
                message = "Staff not found.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(staff.FullName) ||
                string.IsNullOrWhiteSpace(staff.Email))
            {
                message = "Full name and email are required.";
                return false;
            }

            if (!IsValidFullName(staff.FullName.Trim()))
            {
                message = "Full name must contain letters only.";
                return false;
            }

            if (!IsValidEmail(staff.Email.Trim()))
            {
                message = "Please enter a valid email address.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(staff.Phone) && !IsValidPhone(staff.Phone.Trim()))
            {
                message = "Phone number must be 10 digits and start with 0.";
                return false;
            }

            using var context = new IPhoneInventoryDbContext();
            string normalizedEmail = staff.Email.Trim().ToLowerInvariant();

            bool emailExists =
                context.Admins.Any(x => x.Email != null && x.Email.ToLower() == normalizedEmail) ||
                context.Staff.Any(x => x.StaffId != staff.StaffId && x.Email != null && x.Email.ToLower() == normalizedEmail) ||
                context.Customers.Any(x => x.Email != null && x.Email.ToLower() == normalizedEmail);

            if (emailExists)
            {
                message = "This email is already in use.";
                return false;
            }

            existingStaff.FullName = staff.FullName.Trim();
            existingStaff.Email = normalizedEmail;
            existingStaff.Phone = string.IsNullOrWhiteSpace(staff.Phone) ? null : staff.Phone.Trim();
            existingStaff.Status = NormalizeStatus(staff.Status);

            if (!string.IsNullOrWhiteSpace(staff.Password))
            {
                string plainPassword = staff.Password.Trim();

                if (!IsValidPassword(plainPassword))
                {
                    message = "Password must contain at least 8 characters, 1 uppercase letter, 1 number, 1 special character, and no spaces.";
                    return false;
                }

                existingStaff.Password = PasswordHasher.Hash(plainPassword);
            }

            try
            {
                _staffRepository.UpdateStaff(existingStaff);
                message = "Staff account updated successfully.";
                return true;
            }
            catch
            {
                message = "Unable to update staff account.";
                return false;
            }
        }

        public bool ChangeStaffStatus(int staffId, string status, out string message)
        {
            var existingStaff = _staffRepository.GetStaffById(staffId);
            if (existingStaff == null)
            {
                message = "Staff not found.";
                return false;
            }

            string normalizedStatus = NormalizeStatus(status);

            if (normalizedStatus != "Active" && normalizedStatus != "Inactive")
            {
                message = "Status must be Active or Inactive.";
                return false;
            }

            try
            {
                _staffRepository.UpdateStatus(staffId, normalizedStatus);
                message = "Staff status updated successfully.";
                return true;
            }
            catch
            {
                message = "Unable to change staff status.";
                return false;
            }
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return "Active";
            }

            string trimmed = status.Trim();
            return char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();
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

            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasDigit && hasSpecial;
        }
    }
}
