using MongoDB.Driver;

namespace DSFramework.Data.MongoDB.Specifications
{
    public abstract class MongoSpecification<TObject> : IMongoSpecification<TObject>
    {
        public virtual bool AdditionalDomainFilteringIsRequired => false;

        public abstract FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder);

        public IMongoSpecification<TObject> And(IMongoSpecification<TObject> specification)
            => new AndMongoSpecification<TObject>(this, specification);

        public IMongoSpecification<TObject> Or(IMongoSpecification<TObject> specification) => new OrMongoSpecification<TObject>(this, specification);

        public IMongoSpecification<TObject> Not() => new NotMongoSpecification<TObject>(this);
    }
}