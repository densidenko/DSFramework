using DS.Domain.Abstractions.Aggregates;

namespace DS.Domain.Abstractions
{
    public interface IDomainEntity : IDomainEntity<string>
    {
    }

    public interface IDomainEntity<TKey> : IHasId<TKey>
    {
        //TKey GetId();
    }
}