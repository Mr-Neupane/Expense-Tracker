namespace TestApplication.ViewModels;

public class BaseVm
{
    public char RecStatus { get; set; } = 'A';
    public int Status { get; set; } = 1;
    public int RecById { get; set; } = -1;
    public string? Remarks { get; set; }
}