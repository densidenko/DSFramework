using System;

namespace DSFramework.Extensions
{
    public static class ExceptionExtensions
    {
        public static string Describe(this Exception e)
        {
            var result = e?.Message;

            if (e?.InnerException != null)
            {
                result += "\n--> " + e.InnerException.Describe();
            }

            return result;
        }
    }
}