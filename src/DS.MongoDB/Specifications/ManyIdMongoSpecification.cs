using CWT.Infrastructure.Domain.Abstractions.Aggregates;
using MongoDB.Driver;

namespace CWT.Infrastructure.Repository.Mongo.Specifications
{
    public class ManyIdMongoSpecification<TObject> : ManyIdMongoSpecification<string, TObject> where TObject : IHasId<string>
    {
        public ManyIdMongoSpecification(params string[] ids) : base(ids)
        {

        }
    }

    public class ManyIdMongoSpecification<TKey, TObject> : MongoSpecification<TObject> where TObject : IHasId<TKey>
    {
        private readonly TKey[] _ids;

        public ManyIdMongoSpecification(params TKey[] ids)
        {
            _ids = ids;
        }

        public override FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder)
        {
            return filterBuilder.In(a => a.Id, _ids);
        }
    }
}