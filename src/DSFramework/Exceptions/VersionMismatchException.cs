using System;

namespace DSFramework.Exceptions
{
    public class VersionMismatchException : Exception
    {
        public const string VERSION_MISMATCH = "VERSION_MISMATCH";

        public VersionMismatchException() : base(VERSION_MISMATCH)
        {
        }

        public VersionMismatchException(string id, int? currentVersion, int? updateVersion) : base(VERSION_MISMATCH)
        {
            Id = id;
            CurrentVersion = currentVersion;
            UpdateVersion = updateVersion;
        }

        public VersionMismatchException(string id, int? currentVersion, int? updateVersion, Exception inner) : base(VERSION_MISMATCH, inner)
        {
            Id = id;
            CurrentVersion = currentVersion;
            UpdateVersion = updateVersion;
        }

        public string Id { get; }
        public int? CurrentVersion { get; }
        public int? UpdateVersion { get; }

        public string GetExceptionMessage()
        {
            return $"Version mismatch. Id={Id}, CurrentVersion={CurrentVersion}, UpdateVersion={UpdateVersion}";
        }
    }
}