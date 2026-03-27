namespace Services.Models;

public class UpdatePasswordRequest
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
