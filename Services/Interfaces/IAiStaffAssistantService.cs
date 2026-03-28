namespace Services.Interfaces;

public interface IAiStaffAssistantService
{
    Task<string> AskAsync(string question, CancellationToken cancellationToken = default);
}
