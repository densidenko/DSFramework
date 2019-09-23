using System.Linq;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public class AndDomainSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, IAndDomainSpecification<TAggregateRoot>
    {
        public AndDomainSpecification(params IDomainSpecification<TAggregateRoot>[] specifications)
        {
            Specifications = specifications;
        }

        public IDomainSpecification<TAggregateRoot>[] Specifications { get; }

        public override bool IsSatisfied(TAggregateRoot obj)
        {
            return !Specifications.Any() || Specifications.All(a => a.IsSatisfied(obj));
        }
    }
}