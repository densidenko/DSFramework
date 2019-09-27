using System;
using System.Threading.Tasks;
using DSFramework.Contracts.Common;
using DSFramework.Contracts.Common.Models;
using DSFramework.Domain.Abstractions.Specifications;

namespace DSFramework.Application.Services
{
    public interface IReadOnlyGenericManager<in TKey, TEntity>
    {
        event Action<TEntity> EntityDeleted;
        event Action<TEntity> EntityChanged;

        Task<TEntity> Get(TKey id);
        Task<TEntity[]> GetMany(TKey[] ids);
        Task<TEntity[]> GetAll();
        Task<TEntity[]> Search(IDomainSpecification<TEntity> specification);
    }

    public interface IGenericManager<TKey, TEntity, TEntityHolder, in TWriteSnapshotCommand> : IReadOnlyGenericManager<TKey, TEntityHolder>, ICanStart
        where TWriteSnapshotCommand : WriteCommand<TKey, TEntity>, new()
    {
        Task<TKey> Update(TWriteSnapshotCommand command);
        Task<BulkWriteResult<TKey>> BulkUpdate(TWriteSnapshotCommand[] commands);
        Task<TEntityHolder> Delete(TKey entityId);
    }

    public interface IGenericManager<TKey, TEntity> : IGenericManager<TKey, TEntity, EntityHolder<TKey, TEntity>, WriteCommand<TKey, TEntity>>
    {

    }

    public interface IGenericManager<TEntity> : IGenericManager<string, TEntity, EntityHolder<TEntity>, WriteCommand<TEntity>>
    {

    }
}