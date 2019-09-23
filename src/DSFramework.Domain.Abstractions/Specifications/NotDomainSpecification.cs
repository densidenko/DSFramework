namespace DSFramework.Domain.Abstractions.Specifications
{
    public class NotDomainSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, INotDomainSpecification<TAggregateRoot>
    {
        public IDomainSpecification<TAggregateRoot> Source { get; }

        public NotDomainSpecification(IDomainSpecification<TAggregateRoot> source)
        {
            Source = source;
        }

        public override bool IsSatisfied(TAggregateRoot obj)
        {
            return !Source.IsSatisfied(obj);
        }
    }
}