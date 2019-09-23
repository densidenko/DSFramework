using System.Linq;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public class OrDomainSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, IOrDomainSpecification<TAggregateRoot>
    {
        public OrDomainSpecification()
        {
        }

        public OrDomainSpecification(params IDomainSpecification<TAggregateRoot>[] specifications)
        {
            Specifications = specifications;
        }

        public IDomainSpecification<TAggregateRoot>[] Specifications { get; }

        public override bool IsSatisfied(TAggregateRoot obj)
        {
            return !Specifications.Any() || Specifications.Any(a => a.IsSatisfied(obj));
        }
    }
}