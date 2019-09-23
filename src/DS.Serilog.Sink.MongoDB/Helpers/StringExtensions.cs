using System.Text.RegularExpressions;

namespace DS.Serilog.Sink.MongoDB.Helpers
{
    public static class StringExtensions
    {
        public static string Pascalize(this string input)
        {
            return Regex.Replace(input, "(?:^|_)(.)", match => match.Groups[1].Value.ToUpper());
        }
    }
}