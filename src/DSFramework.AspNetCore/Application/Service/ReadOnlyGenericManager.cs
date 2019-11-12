using System;
using System.Threading.Tasks;
using DSFramework.Application.Services;
using DSFramework.Domain.Abstractions.Repositories;
using DSFramework.Domain.Abstractions.Specifications;
using DSFramework.Extensions;
using Microsoft.Extensions.Logging;

namespace DSFramework.AspNetCore.Application.Service
{
    public abstract class ReadOnlyGenericManager<TKey, TEntity, TRepository> : IReadOnlyGenericManager<TKey, TEntity>
        where TRepository : IRepository<TKey, TEntity>
    {
        protected readonly IManagerObserver Observer;
        protected readonly TRepository Repository;

        protected ILogger Logger { get; }

        public virtual event Action<TEntity> EntityChanged;
        public virtual event Action<TEntity> EntityDeleted;

        protected ReadOnlyGenericManager(ILoggerFactory loggerFactory, TRepository repository, IManagerObserver observer)
        {
            Logger = loggerFactory.CreateLogger(GetType().ReadableName());
            Repository = repository;
            Observer = observer;
        }

        public virtual async Task<TEntity> Get(TKey id)
        {
            return await Observe(nameof(Get), async () => await Repository.GetAsync(id));
        }

        public virtual async Task<TEntity[]> GetMany(TKey[] ids)
        {
            return await Observe(nameof(GetMany), async () => await Repository.GetManyAsync(ids));
        }

        public virtual async Task<TEntity[]> GetAll()
        {
            return await Observe(nameof(GetAll), async () => await Repository.GetAllAsync());
        }

        public virtual async Task<TEntity[]> Search(IDomainSpecification<TEntity> specification)
        {
            return await Observe(nameof(Search), async () => await Repository.SearchAsync(specification));
        }

        protected virtual async Task<T> Observe<T>(string actionName, Func<Task<T>> action)
        {
            return await Observer.Observe<TEntity, T>(actionName, action);
        }

        protected async Task Observe(string actionName, Func<Task> action)
        {
            await Observer.Observe<TEntity>(actionName, action);
        }
    }
}