using DSFramework.Caching;
using DSFramework.Domain.Abstractions;
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
using DSFramework.Threading;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DSFramework.MongoDB
{
    public abstract class CachedMongoRepository<TKey, TEntity, TDocument> : CachedMongoRepository<TKey, TDocument>
        where TDocument : IHasId<TKey> where TKey : IComparable<TKey>
    {
        protected override string CollectionName { get; }

        protected CachedMongoRepository(
            IMongoDbContext mongoDbContext,
            IMongoSpecificationConverter<TDocument, TDocument> mongoSpecificationConverter,
            ICache<TKey, TDocument> cache,
            ILoggerFactory loggerFactory,
            Settings settings = null)
            : this(mongoDbContext, mongoSpecificationConverter, cache, entity => entity.Id, loggerFactory, settings)
        {
        }

        protected CachedMongoRepository(
            IMongoDbContext mongoDbContext,
            IMongoSpecificationConverter<TDocument, TDocument> mongoSpecificationConverter,
            ICache<TKey, TDocument> cache,
            Func<TDocument, TKey> keySelector,
            ILoggerFactory loggerFactory,
            Settings settings = null)
            : base(mongoDbContext, mongoSpecificationConverter, cache, keySelector, loggerFactory, settings)
        {
            CollectionName = typeof(TEntity).Name.Pluralize().Camelize();
        }

        protected override IMongoCollection<TDocument> GetOperativeCollection()
        {
            return DbContext.Database.GetCollection<TDocument>(CollectionName);
        }
    }

    public abstract class CachedMongoRepository<TKey, TDocument> : IRepositoryBase<TKey, TDocument>, IMongoRepository
        where TDocument : IHasId<TKey> where TKey : IComparable<TKey>
    {
        public class Settings
        {
            public static Settings Default = new Settings();
            public bool DisableOptimisticLock { get; set; }
            public IRepositoryObserver Observer { get; set; } = RepositoryGlobalSettings.Observer;
        }

        private const int DUPLICATE_KEY = 11000;
        private readonly Func<TDocument, TKey> _keySelector;

        private readonly ICache<TKey, TDocument> _cache;

        private readonly FilterDefinitionBuilder<TDocument> _filterBuilder = Builders<TDocument>.Filter;
        private readonly IMongoSpecificationConverter<TDocument, TDocument> _mongoSpecificationConverter;
        private readonly GenericLock<TKey> _lock = new GenericLock<TKey>();
        private readonly IRepositoryObserver _observer;
        private readonly Settings _settings;
        protected ILogger Logger { get; }
        protected IMongoDbContext DbContext { get; }
        protected virtual string CollectionName { get; }

        protected CachedMongoRepository(
            IMongoDbContext mongoDbContext,
            IMongoSpecificationConverter<TDocument, TDocument> mongoSpecificationConverter,
            ICache<TKey, TDocument> cache,
            Func<TDocument, TKey> keySelector,
            ILoggerFactory loggerFactory,
            Settings settings = null)
            : this(mongoDbContext, mongoSpecificationConverter, cache, keySelector, settings)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Logger = loggerFactory.CreateLogger(GetType().ReadableName());
        }

        protected CachedMongoRepository(
            IMongoDbContext mongoDbContext,
            IMongoSpecificationConverter<TDocument, TDocument> mongoSpecificationConverter,
            ICache<TKey, TDocument> cache,
            Func<TDocument, TKey> keySelector,
            ILogger logger,
            Settings settings = null)
            : this(mongoDbContext, mongoSpecificationConverter, cache, keySelector, settings)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private CachedMongoRepository(
            IMongoDbContext mongoDbContext,
            IMongoSpecificationConverter<TDocument, TDocument> mongoSpecificationConverter,
            ICache<TKey, TDocument> cache,
            Func<TDocument, TKey> keySelector,
            Settings settings = null)
        {
            _settings = settings ?? Settings.Default;
            DbContext = mongoDbContext ?? throw new ArgumentNullException(nameof(mongoDbContext));
            CollectionName = GetCollection<TDocument>().CollectionNamespace.CollectionName;
            _mongoSpecificationConverter = mongoSpecificationConverter ?? throw new ArgumentNullException(nameof(mongoSpecificationConverter));
            _cache = cache;
            _keySelector = keySelector;

            if (!_settings.DisableOptimisticLock && !typeof(IHasVersion).GetTypeInfo().IsAssignableFrom(typeof(TDocument)))
            {
                throw new ArgumentException($"Type {typeof(TDocument)} must be inherited from {typeof(IHasVersion)} to support optimistic lock");
            }

            _observer = _settings.Observer ?? NullObserver.Instance;
        }

        public virtual void CreateIndices()
        {
            var collection = GetOperativeCollection();
            DoCreateIndeces(collection, collection.Indexes, Builders<TDocument>.IndexKeys);
        }

        public async Task<TDocument> GetAsync(TKey key)
        {
            using (Logger.CreateTimeGuard())
            {
                if (!_cache.Get(key, out var result))
                {
                    using (await _lock.LockAsync(key))
                    {
                        if (!_cache.Get(key, out result))
                        {
                            await ObserveAsync(async () =>
                                               {
                                                   result = await GetDocumentAsync(key);
                                                   _cache.Add(key, result);

                                                   return string.Empty;
                                               },
                                               (col, elapsed, res) => _observer.OnGet(col, elapsed),
                                               (c, v) => _observer.OnGetFailed(c));
                        }
                    }
                }

                return result;
            }
        }

        public TDocument Update(TDocument document)
        {
            var key = _keySelector(document);

            using (Logger.CreateTimeGuard())
            {
                using (_lock.Lock(key))
                {
                    var id = document.Id;
                    var collection = GetOperativeCollection();

                    if (_settings.DisableOptimisticLock)
                    {
                        UpdateWithoutLock(document, collection, id);
                    }
                    else
                    {
                        UpdateWithOptimisticLock(document, collection, id);
                    }

                    _cache.Add(key, document);
                    OnUpdated(document);

                    return document;
                }
            }
        }

        public async Task<TDocument> UpdateAsync(TDocument document)
        {
            var key = _keySelector(document);

            using (Logger.CreateTimeGuard())
            {
                using (await _lock.LockAsync(key))
                {
                    var collection = GetOperativeCollection();
                    var id = document.Id;

                    if (_settings.DisableOptimisticLock)
                    {
                        await UpdateWithoutLockAsync(document, collection, id);
                    }
                    else
                    {
                        await UpdateWithOptimisticLockAsync(document, collection, id);
                    }

                    _cache.Add(key, document);
                    OnUpdated(document);

                    return document;
                }
            }
        }

        public TDocument Get(TKey key)
        {
            using (Logger.CreateTimeGuard())
            {
                if (!_cache.Get(key, out var result))
                {
                    using (_lock.Lock(key))
                    {
                        if (!_cache.Get(key, out result))
                        {
                            Observe(() =>
                                    {
                                        result = GetDocument(key);
                                        _cache.Add(key, result);

                                        return string.Empty;
                                    },
                                    (col, elapsed, res) => _observer.OnGet(col, elapsed),
                                    (c, v) => _observer.OnGetFailed(c));
                        }
                    }
                }

                return result;
            }
        }

        public async Task BulkUpsertAsync(TDocument[] documents)
        {
            await ObserveAsync(async () =>
                               {
                                   await BulkUpdateAsyncInternal(documents);

                                   using (await _lock.LockAsync(documents.Select(_keySelector)))
                                   {
                                       foreach (var document in documents)
                                       {
                                           var key = _keySelector(document);
                                           _cache.Add(key, document);
                                       }

                                       return string.Empty;
                                   }
                               },
                               (collection, elapsed, res) => _observer.OnBulkUpdate(collection, elapsed, documents.Length),
                               _observer.OnBulkUpdateFailed);
        }

        public TDocument Delete(TKey key)
        {
            using (Logger.CreateTimeGuard())
            {
                using (_lock.Lock(key))
                {
                    return Observe(() =>
                                   {
                                       var collection = GetOperativeCollection();
                                       var document = collection.FindOneAndDelete(FindByKey(key));
                                       _cache.Remove(key);

                                       return document;
                                   },
                                   (col, elapsed, res) => _observer.OnDelete(col, elapsed),
                                   _observer.OnDeleteFailed);
                }
            }
        }

        public async Task<TDocument> DeleteAsync(TKey key)
        {
            using (Logger.CreateTimeGuard())
            {
                using (await _lock.LockAsync(key))
                {
                    return await ObserveAsync(async () =>
                                              {
                                                  var collection = GetOperativeCollection();
                                                  var document = await collection.FindOneAndDeleteAsync(FindByKey(key));
                                                  _cache.Remove(key);

                                                  return document;
                                              },
                                              (c, t, r) => _observer.OnDelete(c, t),
                                              _observer.OnDeleteFailed);
                }
            }
        }

        public Task<TDocument[]> DeleteManyAsync(TKey[] keys)
        {
            using (Logger.CreateTimeGuard())
            {
                return ObserveAsync(async () =>
                                    {
                                        var mongoSpecification = new ManyIdMongoSpecification<TKey, TDocument>(keys);
                                        var collection = GetOperativeCollection();

                                        var documents = await SearchMongoAsync(mongoSpecification, null, null);

                                        using (await _lock.LockAsync(keys))
                                        {
                                            var filter = mongoSpecification.BuildFilter(Builders<TDocument>.Filter);

                                            await collection.DeleteManyAsync(filter);
                                            foreach (var id in keys)
                                            {
                                                _cache.Remove(id);
                                            }

                                            return documents;
                                        }
                                    },
                                    (collection, elapsed, res) => _observer.OnDeleteMany(collection, elapsed, res?.Length),
                                    _observer.OnDeleteManyFailed);
            }
        }

        public TDocument[] DeleteMany(IDomainSpecification<TDocument> specification)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(() =>
                               {
                                   var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                                   var collection = GetOperativeCollection();
                                   var documents = SearchMongo(mongoSpecification, specification);
                                   var keys = documents.Select(a => _keySelector(a)).OrderBy(key => key).ToArray();

                                   using (_lock.Lock(keys))
                                   {
                                       var ids = documents.Select(a => a.Id).ToArray();
                                       var manyIdMongoSpecification = new ManyIdMongoSpecification<TKey, TDocument>(ids);
                                       var filter = manyIdMongoSpecification.BuildFilter(_filterBuilder);

                                       collection.DeleteMany(filter);

                                       foreach (var key in keys)
                                       {
                                           _cache.Remove(key);
                                       }

                                       return documents;
                                   }
                               },
                               (collection, elapsed, res) => _observer.OnDeleteMany(collection, elapsed, res?.Length),
                               _observer.OnDeleteManyFailed);
            }
        }

        public async Task<TDocument[]> DeleteManyAsync(IDomainSpecification<TDocument> specification)
        {
            using (Logger.CreateTimeGuard())
            {
                var count = -1;
                return await ObserveAsync(async () =>
                                          {
                                              var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                                              var collection = GetOperativeCollection();

                                              var documents = await SearchMongoAsync(mongoSpecification, specification, null);
                                              var ids = documents.Select(root => root.Id).OrderBy(s => s).ToArray();

                                              using (await _lock.LockAsync(ids))
                                              {
                                                  var manyIdMongoSpecification = new ManyIdMongoSpecification<TKey, TDocument>(ids);
                                                  var filter = manyIdMongoSpecification.BuildFilter(Builders<TDocument>.Filter);

                                                  await collection.DeleteManyAsync(filter);

                                                  foreach (var id in ids)
                                                  {
                                                      _cache.Remove(id);
                                                  }

                                                  count = documents.Length;
                                                  return documents;
                                              }
                                          },
                                          (collection, elapsed, res) => _observer.OnDeleteMany(collection, elapsed, res?.Length),
                                          _observer.OnDeleteManyFailed);
            }
        }

        public TDocument[] DeleteMany(TKey[] ids)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(() =>
                               {
                                   var mongoSpecification = new ManyIdMongoSpecification<TKey, TDocument>(ids);
                                   var collection = GetOperativeCollection();
                                   var documents = SearchMongo(mongoSpecification, null);
                                   var keys = documents.Select(a => _keySelector(a)).OrderBy(key => key).ToArray();

                                   using (_lock.Lock(keys))
                                   {
                                       var filter = mongoSpecification.BuildFilter(_filterBuilder);

                                       collection.DeleteMany(filter);

                                       foreach (var key in keys)
                                       {
                                           _cache.Remove(key);
                                       }

                                       return documents;
                                   }
                               },
                               (col, elapsed, res) => _observer.OnDeleteMany(col, elapsed, res?.Length),
                               _observer.OnDeleteManyFailed);
            }
        }

        public TDocument[] Search(IDomainSpecification<TDocument> specification)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(() =>
                               {
                                   var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                                   return SearchMongo(mongoSpecification, specification);
                               },
                               (col, elapsed, res) => _observer.OnSearch(col, elapsed, res?.Length),
                               (c, v) => _observer.OnSearchFailed(c));
            }
        }

        public SearchResult<TDocument> Search(IDomainSpecification<TDocument> specification, SearchOptions searchOptions)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(() =>
                               {
                                   var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                                   var result = SearchMongo(mongoSpecification, specification, searchOptions);
                                   return result;
                               },
                               (col, elapsed, res) => _observer.OnSearch(col, elapsed, res?.Items?.Length),
                               (c, v) => _observer.OnSearchFailed(c));
            }
        }

        public long Count(IDomainSpecification<TDocument> specification)
        {
            using (Logger.CreateTimeGuard())
            {
                var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                return CountMongo(mongoSpecification, SearchOptions.NoLimits);
            }
        }

        public long Count(IDomainSpecification<TDocument> specification, SearchOptions searchOptions)
        {
            using (Logger.CreateTimeGuard())
            {
                var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                return CountMongo(mongoSpecification, searchOptions);
            }
        }

        public async Task<TDocument[]> SearchAsync(IDomainSpecification<TDocument> specification, SearchOptions searchOptions = null)
        {
            using (Logger.CreateTimeGuard())
            {
                return await ObserveAsync(() =>
                                          {
                                              var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
                                              return SearchMongoAsync(mongoSpecification, specification, searchOptions);
                                          },
                                          (col, elapsed, res) => _observer.OnSearch(col, elapsed, res?.Length),
                                          (c, v) => _observer.OnSearchFailed(c));
            }
        }

        public async Task<long> CountAsync(IDomainSpecification<TDocument> specification)
        {
            var mongoSpecification = _mongoSpecificationConverter.Convert(specification);
            return await CountMongoAsync(mongoSpecification);
        }

        public TDocument[] GetAll()
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(() => SearchMongo(null, null),
                               (collection, elapsed, res) => _observer.OnGetAll(collection, elapsed, res?.Length),
                               (c, v) => _observer.OnGetAllFailed(c));
            }
        }

        public Task<TDocument[]> GetAllAsync()
        {
            using (Logger.CreateTimeGuard())
            {
                return ObserveAsync(() => SearchMongoAsync(null, null, null),
                                    (collection, elapsed, res) => _observer.OnGetAll(collection, elapsed, res?.Length),
                                    (c, v) => _observer.OnGetAllFailed(c));
            }
        }

        public TDocument[] GetMany(params TKey[] keys)
        {
            using (Logger.CreateTimeGuard())
            {
                return Observe(() =>
                               {
                                   var result = new List<TDocument>();
                                   var missedKeys = new List<TKey>();

                                   foreach (var id in keys.Distinct())
                                   {
                                       if (_cache.Get(id, out var document))
                                       {
                                           result.Add(document);
                                       }
                                       else
                                       {
                                           missedKeys.Add(id);
                                       }
                                   }

                                   if (missedKeys.Count > 0)
                                   {
                                       using (_lock.Lock(missedKeys))
                                       {
                                           for (var i = missedKeys.Count - 1; i >= 0; i--)
                                           {
                                               if (_cache.Get(missedKeys[i], out var document))
                                               {
                                                   result.Add(document);
                                                   missedKeys.RemoveAt(i);
                                               }
                                           }

                                           if (missedKeys.Count > 0)
                                           {
                                               var roots = SearchMongo(new ManyIdMongoSpecification<TKey, TDocument>(missedKeys.ToArray()), null);

                                               foreach (var root in roots)
                                               {
                                                   result.Add(root);
                                                   _cache.Add(_keySelector(root), root);
                                               }
                                           }
                                       }
                                   }

                                   return result.Where(root => root != null).ToArray();
                               },
                               (col, elapsed, res) => _observer.OnGetMany(col, elapsed, res?.Length),
                               (c, v) => _observer.OnGetManyFailed(c));
            }
        }

        public async Task<TDocument[]> GetManyAsync(params TKey[] keys)
        {
            using (Logger.CreateTimeGuard())
            {
                return await ObserveAsync(async () =>
                                          {
                                              var result = new List<TDocument>();
                                              var missedKeys = new List<TKey>();

                                              foreach (var id in keys.Distinct())
                                              {
                                                  if (_cache.Get(id, out var document))
                                                  {
                                                      result.Add(document);
                                                  }
                                                  else
                                                  {
                                                      missedKeys.Add(id);
                                                  }
                                              }

                                              if (missedKeys.Count > 0)
                                              {
                                                  using (await _lock.LockAsync(missedKeys))
                                                  {
                                                      for (var i = missedKeys.Count - 1; i >= 0; i--)
                                                      {
                                                          if (_cache.Get(missedKeys[i], out var document))
                                                          {
                                                              result.Add(document);
                                                              missedKeys.RemoveAt(i);
                                                          }
                                                      }

                                                      if (missedKeys.Count > 0)
                                                      {
                                                          var roots = await SearchMongoAsync(new ManyIdMongoSpecification<TKey, TDocument>(missedKeys.ToArray()),
                                                                                             null,
                                                                                             null);

                                                          foreach (var root in roots)
                                                          {
                                                              result.Add(root);
                                                              _cache.Add(_keySelector(root), root);
                                                          }
                                                      }
                                                  }
                                              }

                                              return result.Where(root => root != null).ToArray();
                                          },
                                          (col, elapsed, res) => _observer.OnGetMany(col, elapsed, res?.Length),
                                          (c, v) => _observer.OnGetManyFailed(c));
            }
        }

        public void BulkUpsert(TDocument[] documents)
        {
            Observe(() =>
                    {
                        using (_lock.Lock(documents.Select(_keySelector)))
                        {
                            BulkUpdateInternal(documents);

                            foreach (var document in documents)
                            {
                                _cache.Add(_keySelector(document), document);
                            }

                            return string.Empty;
                        }
                    },
                    (col, elapsed, res) => _observer.OnBulkUpdate(col, elapsed, documents?.Length),
                    _observer.OnBulkUpdateFailed);
        }

        protected virtual void DoCreateIndeces(
            IMongoCollection<TDocument> collection,
            IMongoIndexManager<TDocument> indexes,
            IndexKeysDefinitionBuilder<TDocument> indexKeysBuilder)
        {
        }

        protected TDocument GetDocument(TKey key)
        {
            var collection = GetOperativeCollection();
            var document = collection.Find(FindByKey(key)).FirstOrDefault();
            return document;
        }

        protected async Task<TDocument> GetDocumentAsync(TKey key)
        {
            var collection = GetOperativeCollection();
            var document = (await collection.FindAsync(FindByKey(key))).FirstOrDefault();
            return document;
        }

        protected virtual void OnUpdated(TDocument document)
        {
        }

        protected virtual IMongoCollection<TDocument> GetOperativeCollection()
        {
            return GetCollection<TDocument>();
        }

        protected IMongoCollection<T> GetCollection<T>(string partitionKey = null)
        {
            return DbContext.GetCollection<T>(partitionKey);
        }

        private async Task BulkUpdateAsyncInternal(TDocument[] documents)
        {
            var collection = GetOperativeCollection();

            if (_settings.DisableOptimisticLock)
            {
                var upserts = documents.Select(d => new ReplaceOneModel<TDocument>(FindByKey(d.Id), d)
                {
                    IsUpsert = true
                })
                                       .Cast<WriteModel<TDocument>>();

                try
                {
                    await collection.BulkWriteAsync(upserts,
                                                    new BulkWriteOptions
                                                    {
                                                        IsOrdered = false
                                                    });
                }
                catch (MongoBulkWriteException<TDocument> ex)
                {
                    if (ex.WriteErrors.Any(a => a.Code == DUPLICATE_KEY))
                    {
                        throw new VersionMismatchException();
                    }
                }
            }
            else
            {
                var updating = documents.Where(a => IsUpdating((IHasVersion)a)).ToList();
                var creating = documents.Where(a => IsCreating((IHasVersion)a)).ToList();
                var updates = updating.Select(a => a)
                                      .Select(d => new ReplaceOneModel<TDocument>(_filterBuilder.And(FindByKey(d.Id),
                                                                                                     _filterBuilder.Eq(a => ((IHasVersion)a).DataVersion,
                                                                                                                       ((IHasVersion)d).DataVersion - 1)),
                                                                                  d))
                                      .Cast<WriteModel<TDocument>>();
                var creates = creating.Select(d => new InsertOneModel<TDocument>(d));

                try
                {
                    var result = await collection.BulkWriteAsync(updates.Concat(creates),
                                                                 new BulkWriteOptions
                                                                 {
                                                                     IsOrdered = false
                                                                 });

                    if (result.InsertedCount != creating.Count || result.ModifiedCount != updating.Count)
                    {
                        throw new VersionMismatchException();
                    }
                }
                catch (MongoBulkWriteException<TDocument> ex)
                {
                    if (ex.WriteErrors.Any(a => a.Code == DUPLICATE_KEY))
                    {
                        throw new VersionMismatchException();
                    }
                }
            }
        }

        private void BulkUpdateInternal(TDocument[] documents)
        {
            var collection = GetOperativeCollection();

            if (_settings.DisableOptimisticLock)
            {
                var upserts = documents.Select(d => new ReplaceOneModel<TDocument>(FindByKey(d.Id), d)
                {
                    IsUpsert = true
                })
                                       .Cast<WriteModel<TDocument>>();

                try
                {
                    collection.BulkWrite(upserts,
                                         new BulkWriteOptions
                                         {
                                             IsOrdered = false
                                         });
                }
                catch (MongoBulkWriteException<TDocument> ex)
                {
                    if (ex.WriteErrors.Any(a => a.Code == DUPLICATE_KEY))
                    {
                        throw new VersionMismatchException();
                    }
                }
            }
            else
            {
                var updating = documents.Where(a => IsUpdating((IHasVersion)a)).ToList();
                var creating = documents.Where(a => IsCreating((IHasVersion)a)).ToList();
                var updates = updating.Select(a => a)
                                      .Select(d => new ReplaceOneModel<TDocument>(_filterBuilder.And(FindByKey(d.Id),
                                                                                                     _filterBuilder.Eq(a => ((IHasVersion)a).DataVersion,
                                                                                                                       ((IHasVersion)d).DataVersion - 1)),
                                                                                  d))
                                      .Cast<WriteModel<TDocument>>();
                var creates = creating.Select(d => new InsertOneModel<TDocument>(d));

                try
                {
                    var result = collection.BulkWrite(updates.Concat(creates),
                                                      new BulkWriteOptions
                                                      {
                                                          IsOrdered = false
                                                      });

                    if (result.InsertedCount != creating.Count || result.ModifiedCount != updating.Count)
                    {
                        throw new VersionMismatchException();
                    }
                }
                catch (MongoBulkWriteException<TDocument> ex)
                {
                    if (ex.WriteErrors.Any(a => a.Code == DUPLICATE_KEY))
                    {
                        throw new VersionMismatchException();
                    }
                }
            }
        }

        private void UpdateWithOptimisticLock(TDocument document, IMongoCollection<TDocument> collection, TKey key)
        {
            var versionedDoc = (IHasVersion)document;

            if (versionedDoc.DataVersion < Constants.INITIAL_DATA_VERSION)
            {
                throw new ArgumentException($"Invalid DataVersion. DataVersion={versionedDoc.DataVersion}", nameof(versionedDoc.DataVersion));
            }

            if (IsUpdating(versionedDoc))
            {
                Observe(() =>
                        {
                            var result = collection.ReplaceOne(_filterBuilder.And(FindByKey(document.Id),
                                                                                  _filterBuilder.Eq(data => ((IHasVersion)data).DataVersion,
                                                                                                    versionedDoc.DataVersion - 1)),
                                                               document);

                            if (result.ModifiedCount == 0)
                            {
                                var existedDoc = GetDocument(key);
                                throw new VersionMismatchException(document.Id.ToString(),
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
                Observe(() =>
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
                                throw new VersionMismatchException(document.Id.ToString(),
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

        private async Task UpdateWithOptimisticLockAsync(TDocument document, IMongoCollection<TDocument> collection, TKey key)
        {
            var versionedDoc = (IHasVersion)document;

            if (versionedDoc.DataVersion < Constants.INITIAL_DATA_VERSION)
            {
                throw new ArgumentException($"Invalid DataVersion. DataVersion={versionedDoc.DataVersion}", nameof(versionedDoc.DataVersion));
            }

            if (IsUpdating(versionedDoc))
            {
                await ObserveAsync(async () =>
                                   {
                                       var result = await collection.ReplaceOneAsync(_filterBuilder.And(FindByKey(document.Id),
                                                                                                        _filterBuilder.Eq(data => ((IHasVersion)data)
                                                                                                                              .DataVersion,
                                                                                                                          versionedDoc.DataVersion -
                                                                                                                          1)),
                                                                                     document);

                                       if (result.ModifiedCount == 0)
                                       {
                                           var existedDoc = await GetDocumentAsync(key);
                                           throw new VersionMismatchException(document.Id.ToString(),
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
                await ObserveAsync(async () =>
                                   {
                                       try
                                       {
                                           await collection.InsertOneAsync(document);
                                       }
                                       catch (MongoWriteException ex)
                                       {
                                           if (ex.WriteError.Code != DUPLICATE_KEY)
                                           {
                                               throw;
                                           }

                                           var existedDoc = await GetDocumentAsync(key);
                                           throw new VersionMismatchException(document.Id.ToString(),
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

        private void UpdateWithoutLock(TDocument document, IMongoCollection<TDocument> collection, TKey key)
        {
            Observe(() =>
                    {
                        collection.ReplaceOne(FindByKey(key), document, new UpdateOptions { IsUpsert = true });
                        return string.Empty;
                    },
                    (col, elapsed, res) => _observer.OnUpdate(col, elapsed),
                    _observer.OnUpdateFailed);
        }

        private async Task UpdateWithoutLockAsync(TDocument document, IMongoCollection<TDocument> collection, TKey key)
        {
            await ObserveAsync(async () =>
                               {
                                   await collection.ReplaceOneAsync(FindByKey(key), document, new UpdateOptions { IsUpsert = true });
                                   return string.Empty;
                               },
                               (col, elapsed, res) => _observer.OnUpdate(col, elapsed),
                               _observer.OnUpdateFailed);
        }

        private static bool IsUpdating(IHasVersion document)
        {
            return document.DataVersion > Constants.INITIAL_DATA_VERSION;
        }

        private static bool IsCreating(IHasVersion document)
        {
            return document.DataVersion == Constants.INITIAL_DATA_VERSION;
        }

        private SearchResult<TDocument> SearchMongo(
            IMongoSpecification<TDocument> specification,
            IDomainSpecification<TDocument> domainSpecification,
            SearchOptions searchOptions)
        {
            var filter = specification != null ? specification.BuildFilter(Builders<TDocument>.Filter) : Builders<TDocument>.Filter.Empty;
            var collection = GetOperativeCollection();
            ICollection<TDocument> documents;
            bool hasMore;

            if (searchOptions.Limit == int.MaxValue)
            {
                hasMore = false;
                documents = collection.Aggregate(new AggregateOptions { AllowDiskUse = true }).Match(filter).ToList();
            }
            else
            {
                if (searchOptions.Limit <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(searchOptions.Limit), searchOptions.Limit, "Limit is wrong");
                }

                var biggerLimit = searchOptions.Limit + 1;
                documents = collection.Aggregate(new AggregateOptions { AllowDiskUse = true }).Match(filter).Limit(biggerLimit).ToList();
                hasMore = documents.Count > searchOptions.Limit;
                documents = documents.Take(documents.Count > searchOptions.Limit ? searchOptions.Limit : documents.Count).ToList();
            }

            if (specification?.AdditionalDomainFilteringIsRequired == true)
            {
                if (domainSpecification == null)
                {
                    throw new ArgumentNullException(nameof(domainSpecification), "Cannot filter without domain specification");
                }
            }

            return new SearchResult<TDocument>(documents.ToArray(), hasMore);
        }

        private TDocument[] SearchMongo(IMongoSpecification<TDocument> specification, IDomainSpecification<TDocument> domainSpecification)
        {
            var filter = specification != null ? specification.BuildFilter(Builders<TDocument>.Filter) : Builders<TDocument>.Filter.Empty;
            var collection = GetOperativeCollection();
            var documents = collection.Aggregate(new AggregateOptions { AllowDiskUse = true }).Match(filter).ToList();

            if (specification?.AdditionalDomainFilteringIsRequired == true)
            {
                if (domainSpecification == null)
                {
                    throw new ArgumentNullException(nameof(domainSpecification), "Cannot filter without domain specification");
                }

                var filtered = documents.Where(domainSpecification.IsSatisfied).ToArray();

                return filtered;
            }

            return documents.ToArray();
        }

        private async Task<TDocument[]> SearchMongoAsync(
            IMongoSpecification<TDocument> specification,
            IDomainSpecification<TDocument> domainSpecification,
            SearchOptions searchOptions)
        {
            var filter = specification != null ? specification.BuildFilter(Builders<TDocument>.Filter) : Builders<TDocument>.Filter.Empty;
            var collection = GetOperativeCollection();

            var documentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<TDocument>();
            var match = new BsonDocument
            {
                {
                    "$match",
                    filter.Render(documentSerializer, BsonSerializer.SerializerRegistry)
                }
            };
            var pipeline = new List<BsonDocument> { match };

            if (searchOptions?.SortOptions != null && !searchOptions.SortOptions.IsEmpty())
            {
                pipeline.Add(new SortBuilder().Build(searchOptions.SortOptions));
            }

            var result = (await collection.AggregateAsync<TDocument>(pipeline, new AggregateOptions { AllowDiskUse = true })).ToList();

            if (specification?.AdditionalDomainFilteringIsRequired == true)
            {
                if (domainSpecification == null)
                {
                    throw new ArgumentNullException(nameof(domainSpecification), "Cannot filter without domain specification");
                }

                return result.Where(domainSpecification.IsSatisfied).ToArray();
            }

            var res = result.ToArray();
            return res;
        }

        private long CountMongo(IMongoSpecification<TDocument> specification, SearchOptions searchOptions)
        {
            var filter = specification != null ? specification.BuildFilter(Builders<TDocument>.Filter) : Builders<TDocument>.Filter.Empty;
            var collection = GetOperativeCollection();

            if (searchOptions.Limit == int.MaxValue)
            {
                return collection.CountDocuments(filter);
            }

            if (searchOptions.Limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(searchOptions.Limit), searchOptions.Limit, "Limit is wrong");
            }

            return collection.CountDocuments(filter, new CountOptions { Limit = searchOptions.Limit });
        }

        private async Task<long> CountMongoAsync(IMongoSpecification<TDocument> specification)
        {
            var filter = specification != null ? specification.BuildFilter(Builders<TDocument>.Filter) : Builders<TDocument>.Filter.Empty;
            var collection = GetOperativeCollection();
            return await collection.CountDocumentsAsync(filter);
        }

        private FilterDefinition<TDocument> FindByKey(TKey key)
        {
            return _filterBuilder.Eq(data => data.Id, key);
        }

        private T Observe<T>(Func<T> action, Action<string, TimeSpan, T> final, Action<string, bool> onError)
        {
            var sw = Stopwatch.StartNew();
            var res = default(T);

            try
            {
                res = action();
                return res;
            }
            catch (VersionMismatchException)
            {
                onError?.Invoke(CollectionName, true);
                throw;
            }
            catch
            {
                onError?.Invoke(CollectionName, false);
                throw;
            }
            finally
            {
                final?.Invoke(CollectionName, sw.Elapsed, res);
            }
        }

        private async Task<T> ObserveAsync<T>(Func<Task<T>> action, Action<string, TimeSpan, T> final, Action<string, bool> onError)
        {
            var sw = Stopwatch.StartNew();
            var res = default(T);

            try
            {
                res = await action();
                return res;
            }
            catch (VersionMismatchException)
            {
                onError?.Invoke(CollectionName, true);
                throw;
            }
            catch
            {
                onError?.Invoke(CollectionName, false);
                throw;
            }
            finally
            {
                final?.Invoke(CollectionName, sw.Elapsed, res);
            }
        }
    }
}