using System;
using System.Linq;
using DSFramework.Domain.Abstractions.Aggregates;
using DSFramework.Domain.Abstractions.Specifications;

namespace DSFramework.MongoDB.Specifications.Converter
{
    public abstract class
        MongoSpecificationConverter<TAggregateRoot> : MongoSpecificationConverter<string, TAggregateRoot>, IMongoSpecificationConverter<TAggregateRoot>
        where TAggregateRoot : IHasId
    {
    }

    public abstract class
        MongoSpecificationConverter<TKey, TAggregateRoot> : IMongoSpecificationConverter<TAggregateRoot, TAggregateRoot>
        where TAggregateRoot : IHasId<TKey>
    {
        public virtual IMongoSpecification<TAggregateRoot> Convert(IDomainSpecification<TAggregateRoot> source)
        {
            switch (source)
            {
                case null:
                    return null;
                case IManyIdSpecification<TKey, TAggregateRoot> manyIdSpecification:
                    return new ManyIdMongoSpecification<TKey, TAggregateRoot>(manyIdSpecification.Ids);
                case IAndDomainSpecification<TAggregateRoot> and:
                    return new AndMongoSpecification<TAggregateRoot>(and.Specifications.Select(Convert).ToArray());
                case IOrDomainSpecification<TAggregateRoot> or:
                    return new OrMongoSpecification<TAggregateRoot>(or.Specifications.Select(Convert).ToArray());
                case INotDomainSpecification<TAggregateRoot> not:
                    return new NotMongoSpecification<TAggregateRoot>(Convert(not.Source));
                default:
                    throw new NotSupportedException($"Specification {source} cannot be converted");
            }
        }

        public IDomainSpecification<TAggregateRoot> Convert(IMongoSpecification<TAggregateRoot> source)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class
        MongoSpecificationConverter<TKey, TAggregateRoot, TAggregateRootData> : MongoSpecificationConverter<TKey, TKey, TAggregateRoot,
            TAggregateRootData> where TAggregateRootData : IHasId<TKey>
    {
        protected MongoSpecificationConverter() : base(key => key)
        {
        }
    }

    public abstract class
        MongoSpecificationConverter<TDomainKey, TDataKey, TAggregateRoot, TAggregateRootData> : IMongoSpecificationConverter<TAggregateRoot,
            TAggregateRootData>
        where TAggregateRootData : IHasId<TDataKey>
    {
        private readonly Func<TDomainKey, TDataKey> _keyConverter;

        protected MongoSpecificationConverter(Func<TDomainKey, TDataKey> keyConverter)
        {
            _keyConverter = keyConverter ?? throw new ArgumentNullException(nameof(keyConverter));
        }

        public virtual IMongoSpecification<TAggregateRootData> Convert(IDomainSpecification<TAggregateRoot> source)
        {
            switch (source)
            {
                case null:
                    return null;
                case IManyIdSpecification<TDomainKey, TAggregateRoot> manyIdSpecification:
                    return new ManyIdMongoSpecification<TDataKey, TAggregateRootData>(
                        manyIdSpecification.Ids.Select(key => _keyConverter(key)).ToArray());
                case IAndDomainSpecification<TAggregateRoot> and:
                    return new AndMongoSpecification<TAggregateRootData>(and.Specifications.Select(Convert).ToArray());
                case IOrDomainSpecification<TAggregateRoot> or:
                    return new OrMongoSpecification<TAggregateRootData>(or.Specifications.Select(Convert).ToArray());
                case INotDomainSpecification<TAggregateRoot> not:
                    return new NotMongoSpecification<TAggregateRootData>(Convert(not.Source));
                default:
                    throw new NotSupportedException($"Specification {source} cannot be converted");
            }
        }

        public virtual IDomainSpecification<TAggregateRoot> Convert(IMongoSpecification<TAggregateRootData> source)
        {
            throw new NotImplementedException();
        }
    }
}