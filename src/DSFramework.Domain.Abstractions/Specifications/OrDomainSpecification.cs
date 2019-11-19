using System.Linq;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public class OrDomainSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, IOrDomainSpecification<TAggregateRoot>
    {
        public IDomainSpecification<TAggregateRoot>[] Specifications { get; }

        public OrDomainSpecification()
        { }

        public OrDomainSpecification(params IDomainSpecification<TAggregateRoot>[] specifications)
        {
            Specifications = specifications;
        }

        public override bool IsSatisfied(TAggregateRoot obj) => !Specifications.Any() || Specifications.Any(a => a.IsSatisfied(obj));
    }
}