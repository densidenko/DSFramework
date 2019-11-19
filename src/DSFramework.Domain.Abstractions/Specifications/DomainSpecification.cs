using System.Collections.Generic;
using System.Linq;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public abstract class DomainSpecification<TAggregateRoot> : IDomainSpecification<TAggregateRoot>
    {
        public abstract bool IsSatisfied(TAggregateRoot obj);

        public virtual IEnumerable<TAggregateRoot> Filter(IEnumerable<TAggregateRoot> collection) => collection.Where(IsSatisfied);

        public IAndDomainSpecification<TAggregateRoot> And(IDomainSpecification<TAggregateRoot> specification)
            => new AndDomainSpecification<TAggregateRoot>(this, specification);

        public IOrDomainSpecification<TAggregateRoot> Or(IDomainSpecification<TAggregateRoot> specification)
            => new OrDomainSpecification<TAggregateRoot>(this, specification);

        public INotDomainSpecification<TAggregateRoot> Not() => new NotDomainSpecification<TAggregateRoot>(this);
    }
}