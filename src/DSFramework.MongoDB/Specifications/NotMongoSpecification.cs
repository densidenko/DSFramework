using MongoDB.Driver;
using System;

namespace DSFramework.MongoDB.Specifications
{
    public class NotMongoSpecification<TObject> : MongoSpecification<TObject>
    {
        public NotMongoSpecification(IMongoSpecification<TObject> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (Source.AdditionalDomainFilteringIsRequired)
            {
                throw new NotSupportedException("This specification cannot be used");
            }
        }

        public IMongoSpecification<TObject> Source { get; }

        public override FilterDefinition<TObject> BuildFilter(FilterDefinitionBuilder<TObject> filterBuilder)
        {
            return filterBuilder.Not(Source.BuildFilter(filterBuilder));
        }
    }
}