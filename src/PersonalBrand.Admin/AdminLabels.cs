namespace PersonalBrand.Admin;

public static class AdminLabels
{
    public static string Text(string value) => System.Text.RegularExpressions.Regex.Replace(value, "(?<=[a-z])([A-Z])", " $1");
}
