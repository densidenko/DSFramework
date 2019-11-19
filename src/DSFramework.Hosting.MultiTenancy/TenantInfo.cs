namespace DSFramework.Hosting.MultiTenancy
{
    /// <summary>
    ///     Represents Current Tenant's Information
    /// </summary>
    public class TenantInfo
    {
        public long Id { get; }
        public string Name { get; }
        public string ConnectionString { get; }

        /// <summary>
        ///     Name of the culture like "en" or "en-GB"
        /// </summary>
        public string LanguageName { get; set; }

        public string TimeZoneId { get; set; }
        public string Theme { get; set; }

        public TenantInfo(long id, string name, string connectionString)
        {
            Id = id;
            Name = name;
            ConnectionString = connectionString;
        }
    }
}