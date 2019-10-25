using DSFramework.Domain.Abstractions.Entities;
using System;

namespace DSFramework.Contracts.Common.Models
{
    public interface IEntityHolder : IEntityHolder<string> { }

    public interface IEntityHolder<TIdentity> : IHasVersion, IHasId<TIdentity>
    {
        DateTime CreatedDate { get; set; }
        DateTime? ModifiedDate { get; set; }
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        string Application { get; set; }
    }
}