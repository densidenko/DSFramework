using System;
using System.Threading.Tasks;
using DSFramework.Data.Contracts;
using DSFramework.Data.Contracts.Models;
using DSFramework.Domain.Abstractions.Specifications;

namespace DSFramework.Application.Services
{
    public interface IReadOnlyGenericManager<in TKey, TEntity>
    {
        event Action<TEntity> EntityChanged;
        event Action<TEntity> EntityDeleted;

        Task<TEntity> Get(TKey id);
        Task<TEntity[]> GetAll();
        Task<TEntity[]> GetMany(TKey[] ids);
        Task<TEntity[]> Search(IDomainSpecification<TEntity> specification);
    }

    public interface
        IGenericManager<TKey, TEntity, TEntityHolder, in TWriteSnapshotCommand> : IReadOnlyGenericManager<TKey, TEntityHolder>, ICanStart
        where TWriteSnapshotCommand : WriteCommand<TKey, TEntity>, new()
    {
        Task<BulkWriteResult<TKey>> BulkUpdate(TWriteSnapshotCommand[] commands);
        Task<TEntityHolder> Delete(TKey entityId);
        Task<TKey> Update(TWriteSnapshotCommand command);
    }

    public interface IGenericManager<TKey, TEntity> : IGenericManager<TKey, TEntity, EntityHolder<TKey, TEntity>, WriteCommand<TKey, TEntity>>
    { }

    public interface IGenericManager<TEntity> : IGenericManager<string, TEntity, EntityHolder<TEntity>, WriteCommand<TEntity>>
    { }
}