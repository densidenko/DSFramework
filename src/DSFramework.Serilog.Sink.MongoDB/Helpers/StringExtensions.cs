using System.Text.RegularExpressions;

namespace DSFramework.Serilog.Sink.MongoDB.Helpers
{
    public static class StringExtensions
    {
        public static string Pascalize(this string input)
        {
            return Regex.Replace(input, "(?:^|_)(.)", match => match.Groups[1].Value.ToUpper());
        }
    }
}