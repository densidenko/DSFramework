namespace DSFramework.Domain.Abstractions.Entities
{
    public interface IHasVersion
    {
        int DataVersion { get; }
    }
}