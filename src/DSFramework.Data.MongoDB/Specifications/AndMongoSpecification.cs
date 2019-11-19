using System.Linq;
using MongoDB.Driver;

namespace DSFramework.Data.MongoDB.Specifications
{
    public class AndMongoSpecification<TObject> : MongoSpecification<TObject>
    {
        public override bool AdditionalDomainFilteringIsRequired => Specifications.Any(a => a.AdditionalDomainFilteringIsRequired);

        public IMongoSpecification<TObject>[] Specifications { get; }

        public AndMongoSpecification(params IMongoSpecification<TObject>[] specifications)
        {
            Specifications = specifications;
        }

        public override FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder)
            => filterBuilder.And(Specifications.Select(s => s.BuildFilter(filterBuilder)).ToArray());
    }
}