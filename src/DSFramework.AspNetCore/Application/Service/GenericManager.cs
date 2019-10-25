using System;
using System.Threading.Tasks;
using DSFramework.Application.Services;
using DSFramework.AspNetCore.Extensions;
using DSFramework.Contracts.Common;
using DSFramework.Contracts.Common.Models;
using DSFramework.Domain.Abstractions;
using DSFramework.Domain.Abstractions.Repositories;
using DSFramework.Exceptions;
using DSFramework.Extensions;
using DSFramework.GuardToolkit;
using DSFramework.Utils;
using Microsoft.Extensions.Logging;
using Polly;

namespace DSFramework.AspNetCore.Application.Service
{
    public abstract class GenericManager<TEntity, TRepository>
        : GenericManager<string, TEntity, EntityHolder<TEntity>, WriteCommand<TEntity>, TRepository>
        where TRepository : IRepository<string, EntityHolder<TEntity>>
    {
        protected GenericManager(ILoggerFactory loggerFactory, TRepository repository, IManagerObserver observer)
            : base(loggerFactory, repository, observer)
        {
        }
    }

    public abstract class GenericManager<TKey, TEntity, TRepository>
        : GenericManager<TKey, TEntity, EntityHolder<TKey, TEntity>, WriteCommand<TKey, TEntity>, TRepository>
        where TRepository : IRepository<TKey, EntityHolder<TKey, TEntity>>
    {
        protected GenericManager(ILoggerFactory loggerFactory, TRepository repository, IManagerObserver observer)
            : base(loggerFactory, repository, observer)
        {
        }
    }

    public abstract class GenericManager<TKey, TEntity, TEntityHolder, TWriteSnapshotCommand, TRepository>
        : ReadOnlyGenericManager<TKey, TEntityHolder, TRepository>, IGenericManager<TKey, TEntity, TEntityHolder, TWriteSnapshotCommand>, IDisposable
        where TWriteSnapshotCommand : WriteCommand<TKey, TEntity>, new()
        where TRepository : IRepository<TKey, TEntityHolder>
        where TEntityHolder : EntityHolder<TKey, TEntity>, new()
    {
        public override event Action<TEntityHolder> EntityChanged;
        public override event Action<TEntityHolder> EntityDeleted;

        protected GenericManager(ILoggerFactory loggerFactory, TRepository repository, IManagerObserver observer)
            : base(loggerFactory, repository, observer)
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual async Task<BulkWriteResult<TKey>> BulkUpdate(TWriteSnapshotCommand[] commands)
        {
            return await Observe(nameof(BulkUpdate), async () => await BulkUpdateInternal(commands));
        }

        public virtual async Task<TKey> Update(TWriteSnapshotCommand command)
        {
            return await Observe(nameof(Update), async () => await UpdateInternal(command));
        }

        public virtual async Task<TEntityHolder> Delete(TKey entityId)
        {
            return await Observe(nameof(Delete), async () => await DeleteInternal(entityId));
        }

        public virtual async Task<BulkWriteResult<TKey>> BulkUpdateInternal(TWriteSnapshotCommand[] commands)
        {
            var tasks = commands.RunInBulkhead(async command =>
                                               {
                                                   var result = new BulkWriteResultItem<TKey>();

                                                   try
                                                   {
                                                       await Update(command);
                                                       result.IsOk = true;
                                                       result.EntityId = command.EntityId;
                                                   }
                                                   catch (Exception e)
                                                   {
                                                       result.IsOk = false;
                                                       result.EntityId = command.EntityId;
                                                       result.Error = e.Describe();
                                                   }

                                                   return result;
                                               },
                                               10);

            var items = await Task.WhenAll(tasks);

            return new BulkWriteResult<TKey> { Items = items };
        }

        protected void RaiseEntityChanged(TEntityHolder entityHolder)
        {
            EntityChanged?.Invoke(entityHolder);
        }

        protected void RaiseEntityDeleted(TEntityHolder entityHolder)
        {
            EntityDeleted?.Invoke(entityHolder);
        }

        protected async Task<TEntityHolder> DeleteInternal(TKey entityId)
        {
            var entityType = typeof(TEntity).ReadableName();
            Logger.LogDebug("Delete {EntityType} {EntityId} command handled.", entityType, entityId);

            return await Policy.Handle<OptimisticConcurrencyException>()
                               .WaitAndRetryAsync(10, i => TimeSpan.FromMilliseconds(100))
                               .ExecuteAsync(async () =>
                               {
                                   var holder = await Repository.GetAsync(entityId);

                                   if (holder == null)
                                   {
                                       Logger.LogDebug("{EntityType} {EntityId} already removed", entityType, entityId);
                                       return null;
                                   }

                                   var result = await Repository.DeleteAsync(entityId);
                                   Logger.LogInformation("{EntityType} {EntityId} removed", entityType, entityId);

                                   RaiseEntityDeleted(holder);

                                   return result;
                               });
        }

        protected async Task<TKey> UpdateInternal(TWriteSnapshotCommand command)
        {
            var entityId = command.EntityId;
            var entityType = typeof(TEntity).ReadableName();
            Logger.LogDebug("Update {EntityType} {EntityId} handled. DataVersion={DataVersion}", entityType, entityId, command.DataVersion);

            return await Policy.Handle<OptimisticConcurrencyException>()
                               .WaitAndRetryAsync(10, i => TimeSpan.FromMilliseconds(100))
                               .ExecuteAsync(async () =>
                               {
                                   await PrepareCommandForUpdate(command);
                                   var snapshot = command.CreateSnapshot<TEntityHolder>();
                                   var saved = await Repository.UpdateAsync(snapshot);

                                   RaiseEntityChanged(snapshot);
                                   return saved.Id;
                               });
        }

        /// <summary>
        ///     Sets the value of the command Id if it is not set already.
        /// </summary>
        /// <param name="command">The WriteSnapshotCommand</param>
        /// <returns></returns>
        protected async Task PrepareCommandForUpdate(TWriteSnapshotCommand command)
        {
            Check.NotNull(command, nameof(command));

            var defaultTKey = default(TKey);

            void Initial(TWriteSnapshotCommand cmd)
            {
                cmd.EntityId = IdGenerator.GetId<TKey>();
                cmd.DataVersion = Constants.INITIAL_DATA_VERSION;
                cmd.SetCreatedTime(DateTime.UtcNow);
            }

            if (command.EntityId == null || defaultTKey != null && defaultTKey.Equals(command.EntityId) ||
                command.EntityId is string strKey && string.IsNullOrWhiteSpace(strKey))
            {
                Initial(command);
            }
            else
            {
                var entityId = command.EntityId;
                var holder = await Repository.GetAsync(entityId);
                if (holder != null)
                {
                    var hasDataVersion = command.DataVersion != null;
                    var entityType = typeof(TEntity).ReadableName();
                    var nextDataVersion = holder.DataVersion + 1;
                    command.SetCreatedTime(holder.CreatedDate);

                    if (!hasDataVersion)
                    {
                        command.DataVersion = nextDataVersion;
                    }
                    else
                    {
                        if (command.DataVersion < nextDataVersion)
                        {
                            Logger.LogDebug("Invalid {EntityType} update handled. Wrong DataVersion ({DataVersion} < {NextDataVersion})",
                                            entityType,
                                            command.DataVersion,
                                            nextDataVersion);
                            throw new VersionMismatchException(entityId.ToString(), nextDataVersion - 1, command.DataVersion);
                        }
                    }
                }
                else
                {
                    Initial(command);
                }
            }
        }
    }
}