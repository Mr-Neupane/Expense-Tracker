namespace TestApplication.Enums;

public enum Status
{
    Active = 1,
    Reversed = 2
}

public static class EnumExtensions
{
    public static int ToInt(this Status status)
    {
        return (int)status;
    }
}
