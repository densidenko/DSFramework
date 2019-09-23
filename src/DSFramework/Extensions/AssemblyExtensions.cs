using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DSFramework.Extensions
{
    public static class AssemblyExtensions
    {
        private const int MAX_LENGTH = 60;

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string GetAssemblyDescription(this Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            var info = FileVersionInfo.GetVersionInfo(path);

            var productInfo = info.ProductVersion.Length > MAX_LENGTH ? info.ProductVersion.Substring(0, MAX_LENGTH) : info.ProductVersion;
            return $"{assemblyName.Name} ({productInfo ?? "no version"})";
        }
    }
}