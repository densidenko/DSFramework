using DSFramework.Domain.Abstractions.Converters;
using DSFramework.Domain.Abstractions.Specifications;

namespace DSFramework.MongoDB.Specifications.Converter
{
    public interface
        IMongoSpecificationConverter<TAggregateRoot> : IMongoSpecificationConverter<TAggregateRoot, TAggregateRoot>
    {
    }

    public interface
        IMongoSpecificationConverter<TAggregateRoot, TAggregateRootData> : IConverter<IDomainSpecification<TAggregateRoot>,
            IMongoSpecification<TAggregateRootData>>
    {
    }
}