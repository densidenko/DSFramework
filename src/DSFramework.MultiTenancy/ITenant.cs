namespace DSFramework.MultiTenancy
{
    public interface ITenant
    {
        TenantInfo Value { get; }
        bool HasValue { get; }
    }
}