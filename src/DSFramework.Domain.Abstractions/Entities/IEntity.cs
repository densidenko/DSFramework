namespace DSFramework.Domain.Abstractions.Entities
{
    public interface IEntity : IEntity<string>
    {
    }

    public interface IEntity<out TKey> : IHasId<TKey>
    {
    }
}