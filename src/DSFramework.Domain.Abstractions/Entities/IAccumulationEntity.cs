namespace DSFramework.Domain.Abstractions.Entities
{
    public interface IAccumulationEntity : IAccumulationEntity<string>
    {
    }

    public interface IAccumulationEntity<out TKey> : IEntity<TKey>
    {
        int GetDataVersion();
        IAccumulationEntity<TKey> SetDataVersion(int dataVersion);
    }
}