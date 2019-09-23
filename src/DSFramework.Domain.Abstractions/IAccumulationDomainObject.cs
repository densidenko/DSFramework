namespace DSFramework.Domain.Abstractions
{
    public interface IAccumulationDomainObject : IAccumulationDomainObject<string>
    {
    }

    public interface IAccumulationDomainObject<TKey> : IDomainEntity<TKey>
    {
        int GetDataVersion();
        IAccumulationDomainObject<TKey> SetDataVersion(int dataVersion);
    }
}