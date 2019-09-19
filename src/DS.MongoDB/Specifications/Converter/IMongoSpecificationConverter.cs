using CWT.Infrastructure.Domain.Abstractions.Converters;
using CWT.Infrastructure.Domain.Abstractions.Specifications;

namespace CWT.Infrastructure.Repository.Mongo.Specifications.Converter
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