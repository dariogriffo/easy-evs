namespace EasyEvs.Internal
{
    using System.Text.RegularExpressions;

    internal static class StringExtensions
    {
        internal static string ToSnakeCase(this string s)
        {
            return Regex.Replace(s.Replace(" ", string.Empty), "[A-Z]", "_$0").ToLower().TrimStart('_');
        }
    }
}
