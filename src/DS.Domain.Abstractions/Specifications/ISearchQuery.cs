namespace DS.Domain.Abstractions.Specifications
{
    public interface ISearchQuery<THolder, TSpecification> where TSpecification : ISpecification<THolder>
    {
        TSpecification Specification { get; set; }
    }

    public class SearchQuery<THolder, TSpecification> : ISearchQuery<THolder, TSpecification>
        where TSpecification : ISpecification<THolder>
    {
        public TSpecification Specification { get; set; }
    }
}