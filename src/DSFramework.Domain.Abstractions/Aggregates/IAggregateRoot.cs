namespace DSFramework.Domain.Abstractions.Aggregates
{
    public interface IAggregateRoot : IAggregateRoot<string>
    {
        
    }

    public interface IAggregateRoot<TKey> : IHasId<TKey>
    {
        //TKey GetId();
    }
}