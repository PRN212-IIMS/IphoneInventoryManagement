namespace Services.Models;

public class ProfileOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AuthenticatedUser? UpdatedUser { get; set; }
}
