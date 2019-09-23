namespace DSFramework.Domain.Abstractions.Aggregates
{
    public interface IHasVersion
    {
        int DataVersion { get; }
    }
}