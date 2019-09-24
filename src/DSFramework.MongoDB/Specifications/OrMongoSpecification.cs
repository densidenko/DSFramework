using MongoDB.Driver;
using System.Linq;

namespace DSFramework.MongoDB.Specifications
{
    public class OrMongoSpecification<TObject> : MongoSpecification<TObject>
    {
        public OrMongoSpecification(params IMongoSpecification<TObject>[] specifications)
        {
            Specifications = specifications;
        }

        public IMongoSpecification<TObject>[] Specifications { get; }

        public override bool AdditionalDomainFilteringIsRequired => Specifications.Any(a => a.AdditionalDomainFilteringIsRequired);

        public override FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder)
        {
            return filterBuilder.Or(Specifications.Select(s => s.BuildFilter(filterBuilder)).ToArray());
        }
    }
}