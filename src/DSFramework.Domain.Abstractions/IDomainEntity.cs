using DSFramework.Domain.Abstractions.Aggregates;

namespace DSFramework.Domain.Abstractions
{
    public interface IDomainEntity : IDomainEntity<string>
    {
    }

    public interface IDomainEntity<TKey> : IHasId<TKey>
    {
        //TKey GetId();
    }
}