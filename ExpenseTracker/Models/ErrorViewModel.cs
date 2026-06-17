namespace ExpenseTracker.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowErrorCode => !string.IsNullOrEmpty(ErrorCode);
}
