using System.Collections.Generic;
using DSFramework.Domain.Abstractions.Entities;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public interface IDomainSpecification<TAggregateRoot> : ISpecification<TAggregateRoot>
    {
        IAndDomainSpecification<TAggregateRoot> And(IDomainSpecification<TAggregateRoot> specification);
        IEnumerable<TAggregateRoot> Filter(IEnumerable<TAggregateRoot> collection);
        bool IsSatisfied(TAggregateRoot obj);
        INotDomainSpecification<TAggregateRoot> Not();
        IOrDomainSpecification<TAggregateRoot> Or(IDomainSpecification<TAggregateRoot> specification);
    }

    public interface IAndDomainSpecification<TAggregateRoot> : IDomainSpecification<TAggregateRoot>
    {
        IDomainSpecification<TAggregateRoot>[] Specifications { get; }
    }

    public interface IOrDomainSpecification<TAggregateRoot> : IDomainSpecification<TAggregateRoot>
    {
        IDomainSpecification<TAggregateRoot>[] Specifications { get; }
    }

    public interface INotDomainSpecification<TAggregateRoot> : IDomainSpecification<TAggregateRoot>
    {
        IDomainSpecification<TAggregateRoot> Source { get; }
    }

    public interface IManyIdSpecification<TAggregateRoot> : IDomainSpecification<TAggregateRoot> where TAggregateRoot : IHasId
    {
        string[] Ids { get; }
    }

    public interface IManyIdSpecification<out TKey, TAggregateRoot> : IDomainSpecification<TAggregateRoot>
    {
        TKey[] Ids { get; }
    }
}