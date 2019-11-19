namespace DSFramework.Domain.Abstractions.Entities
{
    public interface IAggregateRoot : IAggregateRoot<string>, IEntity
    { }

    public interface IAggregateRoot<TPrimaryKey> : IEntity
    { }
}