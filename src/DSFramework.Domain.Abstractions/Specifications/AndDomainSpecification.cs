using System.Linq;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public class AndDomainSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, IAndDomainSpecification<TAggregateRoot>
    {
        public IDomainSpecification<TAggregateRoot>[] Specifications { get; }

        public AndDomainSpecification(params IDomainSpecification<TAggregateRoot>[] specifications)
        {
            Specifications = specifications;
        }

        public override bool IsSatisfied(TAggregateRoot obj) => !Specifications.Any() || Specifications.All(a => a.IsSatisfied(obj));
    }
}