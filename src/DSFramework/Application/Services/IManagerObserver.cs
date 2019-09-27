using System;
using System.Threading.Tasks;

namespace DSFramework.Application.Services
{
    public interface IManagerObserver
    {
        Task<TResult> Observe<TEntity, TResult>(string actionName, Func<Task<TResult>> action);
        Task Observe<TEntity>(string actionName, Func<Task> action);
        void ObserveCreated<TEntity>();
    }
}