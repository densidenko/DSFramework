using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DSFramework.Contracts.Common.Models;

[assembly: InternalsVisibleTo("DSFramework.AspNetCore")]
namespace DSFramework.Contracts.Common
{
    public class WriteCommand<TEntity> : WriteCommand<string, TEntity>
    {

    }

    public class WriteCommand<TKey, TEntity>
    {
        public TKey EntityId { get; set; }
        public string Application { get; set; }
        public string User { get; set; }
        public int? DataVersion { get; set; }
        public string OperationId { get; set; }
        public DateTime CreatedTime { get; private set; }
        public DateTime? RemovedTime { get; set; }
        public Dictionary<string, string> UpdateTags { get; set; }

        public TEntity Update { get; set; }

        public TEntityHolder CreateSnapshot<TEntityHolder>() where TEntityHolder : EntityHolder<TKey, TEntity>, new()
        {
            return new TEntityHolder
            {
                DataVersion = DataVersion.GetValueOrDefault(1),
                Id = EntityId,
                Application = Application,
                Data = Update,
                CreatedDate = CreatedTime,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = User
            };
        }

        internal void SetCreatedTime(DateTime dateTime)
        {
            CreatedTime = dateTime;
        }
    }
}