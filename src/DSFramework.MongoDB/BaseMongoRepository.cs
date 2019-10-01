using DSFramework.Domain.Abstractions;
using DSFramework.Domain.Abstractions.Converters;
using DSFramework.Domain.Abstractions.Entities;
using DSFramework.Domain.Abstractions.Repositories;
using DSFramework.Domain.Abstractions.Repositories.Observers;
using DSFramework.Domain.Abstractions.Specifications;
using DSFramework.Exceptions;
using DSFramework.Extensions;
using DSFramework.Logging.Timing;
using DSFramework.MongoDB.Contexts;
using DSFramework.MongoDB.Specifications;
using DSFramework.MongoDB.Specifications.Converter;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DSFramework.MongoDB
{
    public abstract class BaseMongoRepository<TKey, TAggregate> : BaseMongoRepository<TKey, TKey, TAggregate, TAggregate>
        where TKey : IComparable<TKey>
        where TAggregate : IHasId<TKey>
    {
        private class FakeDocumentConverter : IConverter<TAggregate, TAggregate>
        {
            public static readonly FakeDocumentConverter Instance = new FakeDocumentConverter();

            public TAggregate Convert(TAggregate source)
            {
                return source;
            }
        }

        private class FakeKeyConverter : IConverter<TKey, TKey>
        {
            public static readonly FakeKeyConverter Instance = new FakeKeyConverter();

            public TKey Convert(TKey source)
            {
                return source;
            }
        }

        protected BaseMongoRepository(
            IMongoDatabase mongoDatabase,
            ILoggerFactory loggerFactory,
            IMongoSpecificationConverter<TAggregate> mongoSpecificationConverter)
            : this(new MongoDbContext(mongoDatabase), loggerFactory, mongoSpecificationConverter)
        {
        }

        protected BaseMongoRepository(
            IMongoDbContext mongoDbContext,
            ILoggerFactory loggerFactory,
            IMongoSpecificationConverter<TAggregate> mongoSpecificationConverter,
            Settings settings = null)
            : base(
                mongoDbContext,
                loggerFactory,
                FakeDocumentConverter.Instance,
                FakeKeyConverter.Instance,
                mongoSpecificationConverter,
                settings)
        {
        }
    }

    public abstract class BaseMongoRepository<TDomainKey, TDataKey, TAggregateRoot, TAggregateRootData> : IMongoRepository, IRepository<TDomainKey, TAggregateRoot>
        where TDomainKey : IComparable<TDomainKey>
        where TAggregateRootData : IHasId<TDataKey>
    {
        public class Settings
        {
            public static Settings Default = new Settings();
            public bool DisableOptimisticLock { get; set; }
            public IRepositoryObserver Observer { get; set; } = RepositoryGlobalSettings.Observer;
        }

        private const int DUPLICATE_KEY = 11000;

        private readonly string _collectionName;
        private readonly IRepositoryObserver _observer;
        private readonly FilterDefinitionBuilder<TAggregateRootData> _filterBuilder = Builders<TAggregateRootData>.Filter;
        private readonly IMongoSpecificationConverter<TAggregateRoot, TAggregateRootData> _mongoSpecificationConverter;
        private readonly Settings _settings;

        protected ILogger Logger { get; }
        protected IMongoDbContext DbContext { get; }
        protected IConverter<TAggregateRoot, TAggregateRootData> Converter { get; }
        protected IConverter<TDomainKey, TDataKey> KeyConverter { get; }

        protected BaseMongoRepository(
            IMongoDbContext mongoDbContext,
            ILoggerFactory loggerFactory,
            IConverter<TAggregateRoot, TAggregateRootData> converter,
            IConverter<TDomainKey, TDataKey> keyConverter,
            IMongoSpecificationConverter<TAggregateRoot, TAggregateRootData> mongoSpecificationConverter,
            Settings settings = null)
        {
            Logger = loggerFactory.CreateLogger(GetType().ReadableName());
            DbContext = mongoDbContext ?? throw new ArgumentNullException(nameof(mongoDbContext));
            ;
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            KeyConverter = keyConverter ?? throw new ArgumentNullException(nameof(keyConverter));
            _mongoSpecificationConverter = mongoSpecificationConverter ?? throw new ArgumentNullException(nameof(mongoSpecificationConverter));
            _settings = settings ?? new Settings();
            _observer = _settings.Observer ?? NullObserver.Instance;
            _collectionName = GetCollection<TAggregateRoot>().CollectionNamespace.CollectionName;
        }

        public TAggregateRoot Get(TDomainKey key)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(
                    () =>
                    {
                        var document = GetDocument(key);

                        return Converter.Convert(document);
                    },
                    (col, elapsed, res) => _observer.OnGet(col, elapsed),
                    (c, v) => _observer.OnGetFailed(c));
            }
        }

        public TAggregateRoot Update(TAggregateRoot entity)
        {
            using (Logger.CreateTimeGuard())
            {
                var document = Converter.Convert(entity);
                var key = document.Id;
                var collection = GetOperativeCollection();

                if (_settings.DisableOptimisticLock)
                {
                    UpdateWithoutLock(document, collection, key);
                }
                else
                {
                    UpdateWithOptimisticLock(document, collection, key);
                }

                var updatedAggregateRoot = Converter.Convert(document);
                OnUpdated(updatedAggregateRoot, document);

                return updatedAggregateRoot;
            }
        }

        public TAggregateRoot[] GetAll()
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(
                    () => SearchMongo(null, null, out _, SearchOptions.NoLimits),
                    (collection, elapsed, res) => _observer.OnGetAll(collection, elapsed, res?.Items?.Length),
                    (c, v) => _observer.OnGetAllFailed(c)).Items;
            }
        }

        public TAggregateRoot[] Search(IDomainSpecification<TAggregateRoot> specification)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(
                    () =>
                    {
                        var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                        return SearchMongo(mongoSpecification, specification, out _, SearchOptions.NoLimits).Items;
                    },
                    (col, elapsed, res) => _observer.OnSearch(col, elapsed, res?.Length),
                    (c, v) => _observer.OnSearchFailed(c));
            }
        }

        public SearchResult<TAggregateRoot> Search(IDomainSpecification<TAggregateRoot> specification, SearchOptions searchOptions)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(
                    () =>
                    {
                        var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                        var result = SearchMongo(mongoSpecification, specification, out _, searchOptions);
                        return result;
                    },
                    (col, elapsed, res) => _observer.OnSearch(col, elapsed, res?.Items?.Length),
                    (c, v) => _observer.OnSearchFailed(c));
            }
        }

        public virtual void CreateIndices()
        {
            var collection = GetOperativeCollection();
            DoCreateIndices(collection, collection.Indexes, Builders<TAggregateRootData>.IndexKeys);
        }

        protected virtual void DoCreateIndices(
           IMongoCollection<TAggregateRootData> collection,
           IMongoIndexManager<TAggregateRootData> indexes,
           IndexKeysDefinitionBuilder<TAggregateRootData> indexKeysBuilder)
        {
        }

        protected IMongoCollection<TAggregateRootData> GetOperativeCollection()
        {
            return GetCollection<TAggregateRootData>();
        }

        protected IMongoCollection<TDocument> GetCollection<TDocument>(string partitionKey = null)
        {
            return DbContext.GetCollection<TDocument>(partitionKey);
        }

        protected TAggregateRootData GetDocument(TDomainKey key)
        {
            var collection = GetOperativeCollection();

            var document = IFindFluentExtensions.FirstOrDefault(IMongoCollectionExtensions.Find(collection, FindByKey(key)));
            return document;
        }

        protected TAggregateRootData GetDocument(TDataKey key)
        {
            var collection = GetOperativeCollection();

            var document = IFindFluentExtensions.FirstOrDefault(IMongoCollectionExtensions.Find(collection, FindByKey(key)));
            return document;
        }

        protected virtual void OnUpdated(TAggregateRoot entity, TAggregateRootData document)
        {
        }

        private T Observe<T>(Func<T> action, Action<string, TimeSpan, T> final, Action<string, bool> onError)
        {
            var sw = Stopwatch.StartNew();
            T res = default;

            try
            {
                res = action();
                return res;
            }
            catch (VersionMismatchException)
            {
                onError?.Invoke(_collectionName, true);
                throw;
            }
            catch
            {
                onError?.Invoke(_collectionName, false);
                throw;
            }
            finally
            {
                final?.Invoke(_collectionName, sw.Elapsed, res);
            }
        }

        private FilterDefinition<TAggregateRootData> FindByKey(TDomainKey key)
        {
            return _filterBuilder.Eq(data => data.Id, KeyConverter.Convert(key));
        }

        private FilterDefinition<TAggregateRootData> FindByKey(TDataKey key)
        {
            return _filterBuilder.Eq(data => data.Id, key);
        }

        private void UpdateWithOptimisticLock(TAggregateRootData document, IMongoCollection<TAggregateRootData> collection, TDataKey key)
        {
            var versionedDoc = (IHasVersion)document;

            if (versionedDoc.DataVersion < Constants.INITIAL_DATA_VERSION)
            {
                throw new ArgumentException($"Invalid DataVersion. DataVersion={versionedDoc.DataVersion}", nameof(versionedDoc.DataVersion));
            }

            if (IsUpdating(versionedDoc))
            {
                Observe(
                    () =>
                    {
                        var result = collection.ReplaceOne(
                            _filterBuilder.And(
                                FindByKey(document.Id),
                                _filterBuilder.Eq(data => ((IHasVersion)data).DataVersion, versionedDoc.DataVersion - 1)),
                            document);

                        if (result.ModifiedCount == 0)
                        {
                            var existedDoc = GetDocument(key);
                            throw new VersionMismatchException(
                                document.Id.ToString(),
                                existedDoc != null ? ((IHasVersion)existedDoc).DataVersion : -1,
                                versionedDoc.DataVersion);
                        }

                        return string.Empty;
                    },
                    (col, elapsed, res) => _observer.OnUpdate(col, elapsed),
                    _observer.OnUpdateFailed);
            }
            else
            {
                Observe(
                    () =>
                    {
                        try
                        {
                            collection.InsertOne(document);
                        }
                        catch (MongoWriteException ex)
                        {
                            if (ex.WriteError.Code != DUPLICATE_KEY)
                            {
                                throw;
                            }

                            var existedDoc = GetDocument(key);
                            throw new VersionMismatchException(
                                document.Id.ToString(),
                                versionedDoc.DataVersion,
                                ((IHasVersion)existedDoc)?.DataVersion,
                                ex);
                        }

                        return string.Empty;
                    },
                    (col, elapsed, res) => _observer.OnCreate(col, elapsed),
                    _observer.OnCreateFailed);
            }
        }

        private void UpdateWithoutLock(TAggregateRootData document, IMongoCollection<TAggregateRootData> collection, TDataKey key)
        {
            Observe(
                () =>
                {
                    collection.ReplaceOne(FindByKey(key), document, new UpdateOptions { IsUpsert = true });
                    return string.Empty;
                },
                (col, elapsed, res) => _observer.OnUpdate(col, elapsed),
                _observer.OnUpdateFailed);
        }

        private static bool IsUpdating(IHasVersion document)
        {
            return document.DataVersion > Constants.INITIAL_DATA_VERSION;
        }

        private SearchResult<TAggregateRoot> SearchMongo(
            IMongoSpecification<TAggregateRootData> specification,
            IDomainSpecification<TAggregateRoot> domainSpecification,
            out List<TAggregateRootData> docs,
            SearchOptions searchOptions)
        {
            var filter = specification != null
                ? specification.BuildFilter(Builders<TAggregateRootData>.Filter)
                : Builders<TAggregateRootData>.Filter.Empty;
            var collection = GetOperativeCollection();
            ICollection<Tuple<TAggregateRootData, TAggregateRoot>> result;
            bool hasMore;

            if (searchOptions.Limit == int.MaxValue)
            {
                hasMore = false;
                docs = IAsyncCursorSourceExtensions.ToList(
                    IMongoCollectionExtensions.Aggregate(collection, new AggregateOptions { AllowDiskUse = true }).Match(filter));
                result = docs.Select(doc => Tuple.Create(doc, Converter.Convert(doc))).ToArray();
            }

            else
            {
                if (searchOptions.Limit <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(searchOptions.Limit), searchOptions.Limit, "Limit is wrong");
                }

                var biggerLimit = searchOptions.Limit + 1;
                docs = IAsyncCursorSourceExtensions.ToList(
                    IMongoCollectionExtensions.Aggregate(collection, new AggregateOptions { AllowDiskUse = true })
                                              .Match(filter)
                                              .Limit(biggerLimit));
                result = docs.Select(doc => Tuple.Create(doc, Converter.Convert(doc))).ToArray();
                hasMore = result.Count > searchOptions.Limit;
                result = result.Take(result.Count > searchOptions.Limit ? searchOptions.Limit : result.Count).ToList();
            }

            if (specification?.AdditionalDomainFilteringIsRequired == true)
            {
                if (domainSpecification == null)
                {
                    throw new ArgumentNullException(nameof(domainSpecification), "Cannot filter without domain specification");
                }

                result = result.Where(pair => domainSpecification.IsSatisfied(pair.Item2)).ToArray();
                docs = result.Select(a => a.Item1).ToList();
            }

            var res = result.ToArray();
            return new SearchResult<TAggregateRoot>(res.Select(a => a.Item2).ToArray(), hasMore);
        }

        #region Not Implemented Members

        public Task<TAggregateRoot> GetAsync(TDomainKey key)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot[]> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public TAggregateRoot[] GetMany(params TDomainKey[] keys)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot[]> GetManyAsync(params TDomainKey[] keys)
        {
            throw new NotImplementedException();
        }

        public void BulkUpsert(TAggregateRoot[] documents)
        {
            throw new NotImplementedException();
        }

        public Task BulkUpsertAsync(TAggregateRoot[] entities)
        {
            throw new NotImplementedException();
        }

        public TAggregateRoot Delete(TDomainKey key)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot> DeleteAsync(TDomainKey key)
        {
            throw new NotImplementedException();
        }

        public TAggregateRoot[] DeleteMany(TDomainKey[] keys)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot[]> DeleteManyAsync(TDomainKey[] keys)
        {
            throw new NotImplementedException();
        }

        public TAggregateRoot[] DeleteMany(IDomainSpecification<TAggregateRoot> specification)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot[]> DeleteManyAsync(IDomainSpecification<TAggregateRoot> specification)
        {
            throw new NotImplementedException();
        }

        public long Count(IDomainSpecification<TAggregateRoot> specification)
        {
            throw new NotImplementedException();
        }

        public long Count(IDomainSpecification<TAggregateRoot> specification, SearchOptions searchOptions)
        {
            throw new NotImplementedException();
        }

        public Task<TAggregateRoot[]> SearchAsync(IDomainSpecification<TAggregateRoot> specification, SearchOptions searchOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<long> CountAsync(IDomainSpecification<TAggregateRoot> specification)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}