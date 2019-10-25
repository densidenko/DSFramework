namespace DSFramework.Domain.Abstractions.Entities
{
    public interface IHasId : IHasId<string>
    {
    }

    public interface IHasId<out T>
    {
        T Id { get; }
    }
}