using MongoDB.Driver;

namespace CWT.Infrastructure.Repository.Mongo.Specifications
{
    public abstract class MongoSpecification<TObject> : IMongoSpecification<TObject>
    {
        public virtual bool AdditionalDomainFilteringIsRequired => false;

        public abstract FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder);

        public IMongoSpecification<TObject> And(IMongoSpecification<TObject> specification)
        {
            return new AndMongoSpecification<TObject>(this, specification);
        }

        public IMongoSpecification<TObject> Or(IMongoSpecification<TObject> specification)
        {
            return new OrMongoSpecification<TObject>(this, specification);
        }

        public IMongoSpecification<TObject> Not()
        {
            return new NotMongoSpecification<TObject>(this);
        }
    }
}