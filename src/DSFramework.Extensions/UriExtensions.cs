using System;
using System.Linq;

namespace DSFramework.Extensions
{
    public static class UriExtensions
    {
        public static Uri Append(this Uri uri, params string[] paths)
            => new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => $"{current.TrimEnd('/')}/{path.TrimStart('/')}"));
    }
}