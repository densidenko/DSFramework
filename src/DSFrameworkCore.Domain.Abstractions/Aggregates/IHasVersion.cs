namespace DS.Domain.Abstractions.Aggregates
{
    public interface IHasVersion
    {
        int DataVersion { get; }
    }
}