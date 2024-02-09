namespace EasyEvs.Internal;

using System.Text.RegularExpressions;

internal static class StringExtensions
{
    internal static string ToSnakeCase(this string s, char separator = '_')
    {
        return Regex
            .Replace(s.Replace(" ", string.Empty), "[A-Z]", $"{separator}$0")
            .ToLower()
            .TrimStart(separator);
    }
}
