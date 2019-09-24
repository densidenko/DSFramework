using System.Collections.Generic;
using System.Linq;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public abstract class DomainSpecification<TAggregateRoot> : IDomainSpecification<TAggregateRoot>
    {
        public abstract bool IsSatisfied(TAggregateRoot obj);

        public virtual IEnumerable<TAggregateRoot> Filter(IEnumerable<TAggregateRoot> collection)
        {
            return collection.Where(IsSatisfied);
        }

        public IAndDomainSpecification<TAggregateRoot> And(IDomainSpecification<TAggregateRoot> specification)
        {
            return new AndDomainSpecification<TAggregateRoot>(this, specification);
        }

        public IOrDomainSpecification<TAggregateRoot> Or(IDomainSpecification<TAggregateRoot> specification)
        {
            return new OrDomainSpecification<TAggregateRoot>(this, specification);
        }

        public INotDomainSpecification<TAggregateRoot> Not()
        {
            return new NotDomainSpecification<TAggregateRoot>(this);
        }
    }
}