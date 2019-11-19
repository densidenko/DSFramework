using System;
using MongoDB.Driver;

namespace DSFramework.Data.MongoDB.Specifications
{
    public class NotMongoSpecification<TObject> : MongoSpecification<TObject>
    {
        public IMongoSpecification<TObject> Source { get; }

        public NotMongoSpecification(IMongoSpecification<TObject> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (Source.AdditionalDomainFilteringIsRequired)
            {
                throw new NotSupportedException("This specification cannot be used");
            }
        }

        public override FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder)
            => filterBuilder.Not(Source.BuildFilter(filterBuilder));
    }
}