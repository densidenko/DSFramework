namespace DSFramework.Domain.Abstractions.Repositories
{
    public interface IRepository<TAggregate> : IRepository<string, TAggregate>
    { }

    public interface IRepository<in TDomainKey, TAggregate> : IRepositoryBase<TDomainKey, TAggregate>
    { }
}