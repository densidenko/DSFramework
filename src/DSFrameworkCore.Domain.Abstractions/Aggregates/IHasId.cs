namespace DS.Domain.Abstractions.Aggregates
{
    public interface IHasId : IHasId<string>
    {
    }

    public interface IHasId<T>
    {
        T Id { get; }
    }
}