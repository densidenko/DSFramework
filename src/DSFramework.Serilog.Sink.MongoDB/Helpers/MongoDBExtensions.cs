using MongoDB.Bson;
using MongoDB.Driver;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DSFramework.Serilog.Sink.MongoDB.Helpers
{
    public static class MongoDBExtensions
    {
        /// <summary>
        ///     Returns true if a collection exists on the mongodb server.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        internal static bool CollectionExists(this IMongoDatabase database, string collectionName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection != null;
        }

        /// <summary>
        ///     Verifies the collection exists. If it doesn't, create it using the Collection Creation Options provided.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="collectionCreationOptions">The collection creation options.</param>
        internal static void VerifyCollectionExists(
            this IMongoDatabase database,
            string collectionName,
            CreateCollectionOptions collectionCreationOptions = null)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (collectionName == null)
            {
                throw new ArgumentNullException(nameof(collectionName));
            }

            if (!database.CollectionExists(collectionName))
            {
                try
                {
                    database.CreateCollection(collectionName, collectionCreationOptions);
                }
                catch (MongoCommandException ex)
                {
                    if (!ex.ErrorMessage.Equals("Collection already exists."))
                    {
                        throw;
                    }
                }
            }

            var collection = database.GetCollection<BsonDocument>(collectionName);
            EnsureExpireIndexOnTimeStamp(collection, TimeSpan.FromDays(90));
            EnsureHashedIndexOnIpAddress(collection);
            EnsureCompoundIndexOnDate(collection);
            EnsureCompoundIndexOnUser(collection);
            EnsureHashedIndexOnConnectionId(collection);
            EnsureHashedIndexOnRequestId(collection);
            EnsureHashedIndexOnCorrelationId(collection);
        }

        /// <summary>
        ///     Generate BSON documents from LogEvents.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="formatter">The formatter.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        internal static IReadOnlyCollection<BsonDocument> GenerateBsonDocuments(this IEnumerable<LogEvent> events, ITextFormatter formatter)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var payload = new StringWriter();

            payload.Write(@"{""logEvents"":[");

            foreach (var logEvent in events)
            {
                formatter.Format(logEvent, payload);
            }

            payload.Write("]}");

            var bson = BsonDocument.Parse(payload.ToString());
            return bson["logEvents"].AsBsonArray.Select(x => x.AsBsonDocument).ToList();
        }

        private static void EnsureExpireIndexOnTimeStamp(IMongoCollection<BsonDocument> collection, TimeSpan expireAfter)
        {
            var model = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending(IndexKeys.TIMESTAMP_UTC),
                                                           new CreateIndexOptions { Background = true, ExpireAfter = expireAfter });
            collection.Indexes.CreateOneAsync(model);
        }

        private static void EnsureCompoundIndexOnDate(IMongoCollection<BsonDocument> collection)
        {
            var model =
                new
                    CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Combine(Builders<BsonDocument>
                                                                                            .IndexKeys.Ascending(IndexKeys.DATE),
                                                                                            Builders<BsonDocument>
                                                                                                .IndexKeys.Ascending(IndexKeys.APPLICATION),
                                                                                            Builders<BsonDocument>
                                                                                                .IndexKeys.Ascending(IndexKeys.LEVEL),
                                                                                            Builders<BsonDocument>
                                                                                                .IndexKeys.Ascending(IndexKeys.MESSAGE_TEMPLATE)),
                                                   new CreateIndexOptions { Background = true });
            collection.Indexes.CreateOneAsync(model);
        }

        private static void EnsureCompoundIndexOnUser(IMongoCollection<BsonDocument> collection)
        {
            var model =
                new
                    CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Combine(Builders<BsonDocument>
                                                                                            .IndexKeys.Ascending(IndexKeys.DATE),
                                                                                            Builders<BsonDocument>
                                                                                                .IndexKeys.Ascending($"{IndexKeys.EVENT_ID}.Id"),
                                                                                            Builders<BsonDocument>
                                                                                                .IndexKeys.Ascending(IndexKeys.USER)),
                                                   new CreateIndexOptions { Background = true });
            collection.Indexes.CreateOneAsync(model);
        }

        private static void EnsureHashedIndexOnIpAddress(IMongoCollection<BsonDocument> collection)
        {
            var model = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Hashed($"{IndexKeys.PROPERTIES}.{IndexKeys.IP_ADDRESS}"),
                                                           new CreateIndexOptions { Background = true });
            collection.Indexes.CreateOneAsync(model);
        }

        private static void EnsureHashedIndexOnCorrelationId(IMongoCollection<BsonDocument> collection)
        {
            var model =
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Hashed($"{IndexKeys.PROPERTIES}.{IndexKeys.CORRELATION_ID}"),
                                                   new CreateIndexOptions { Background = true });
            collection.Indexes.CreateOneAsync(model);
        }

        private static void EnsureHashedIndexOnRequestId(IMongoCollection<BsonDocument> collection)
        {
            var model = new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Hashed($"{IndexKeys.PROPERTIES}.{IndexKeys.REQUEST_ID}"),
                                                           new CreateIndexOptions { Background = true });
            collection.Indexes.CreateOneAsync(model);
        }

        private static void EnsureHashedIndexOnConnectionId(IMongoCollection<BsonDocument> collection)
        {
            var model =
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Hashed($"{IndexKeys.PROPERTIES}.{IndexKeys.CONNECTION_ID}"),
                                                   new CreateIndexOptions { Background = true });
            collection.Indexes.CreateOneAsync(model);
        }
    }
}