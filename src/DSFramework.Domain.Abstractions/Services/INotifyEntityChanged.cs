using System;

namespace DSFramework.Domain.Abstractions.Services
{
    public interface INotifyEntityChanged<out TEntity>
    {
        event Action<TEntity> EntityChanged;
        event Action<TEntity> EntityDeleted;
    }
}