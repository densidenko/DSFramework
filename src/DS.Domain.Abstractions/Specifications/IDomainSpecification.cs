using System.Collections.Generic;
using DS.Domain.Abstractions.Aggregates;

namespace DS.Domain.Abstractions.Specifications
{
    public interface IDomainSpecification<TAggregateRoot> : ISpecification<TAggregateRoot>
    {
        bool IsSatisfied(TAggregateRoot obj);
        IEnumerable<TAggregateRoot> Filter(IEnumerable<TAggregateRoot> collection);
        IAndDomainSpecification<TAggregateRoot> And(IDomainSpecification<TAggregateRoot> specification);
        IOrDomainSpecification<TAggregateRoot> Or(IDomainSpecification<TAggregateRoot> specification);
        INotDomainSpecification<TAggregateRoot> Not();
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