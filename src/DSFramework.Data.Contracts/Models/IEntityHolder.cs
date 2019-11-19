using System;
using DSFramework.Domain.Abstractions.Entities;

namespace DSFramework.Data.Contracts.Models
{
    public interface IEntityHolder : IEntityHolder<string>
    { }

    public interface IEntityHolder<out TIdentity> : IHasVersion, IHasId<TIdentity>
    {
        DateTime CreatedDate { get; set; }
        DateTime? ModifiedDate { get; set; }
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        string Application { get; set; }
    }
}