using CWT.Infrastructure.Domain.Abstractions.Specifications;
using MongoDB.Driver;

namespace CWT.Infrastructure.Repository.Mongo.Specifications
{
    public interface IMongoSpecification<TDocument> : ISpecification<TDocument>
    {
        bool AdditionalDomainFilteringIsRequired { get; }

        FilterDefinition<TDocument> BuildFilter(FilterDefinitionBuilder<TDocument> filterBuilder);
    }
}