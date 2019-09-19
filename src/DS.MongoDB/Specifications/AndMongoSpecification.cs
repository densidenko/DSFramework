using MongoDB.Driver;
using System.Linq;

namespace CWT.Infrastructure.Repository.Mongo.Specifications
{
    public class AndMongoSpecification<TObject> : MongoSpecification<TObject>
    {
        public AndMongoSpecification(params IMongoSpecification<TObject>[] specifications)
        {
            Specifications = specifications;
        }

        public IMongoSpecification<TObject>[] Specifications { get; }

        public override bool AdditionalDomainFilteringIsRequired =>
            Specifications.Any(a => a.AdditionalDomainFilteringIsRequired);

        public override FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder)
        {
            return filterBuilder.And(Specifications.Select(s => s.BuildFilter(filterBuilder)).ToArray());
        }
    }
}