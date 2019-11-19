using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSFramework.Logging.Serilog.Sink.MongoDB.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace DSFramework.Logging.Serilog.Sink.MongoDB.Sinks
{
    /// <summary>
    ///     Writes log events as documents to a MongoDB database.
    /// </summary>
    public class MongoDBSink : PeriodicBatchingSink
    {
        /// <summary>
        ///     A reasonable default for the number of events posted in
        ///     each batch.
        /// </summary>
        public const int DEFAULT_BATCH_POSTING_LIMIT = 50;

        /// <summary>
        ///     The default name for the log collection.
        /// </summary>
        public const string DEFAULT_COLLECTION_NAME = "log";

        /// <summary>
        ///     A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        private readonly string _collectionName;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ITextFormatter _mongoDbJsonFormatter;

        /// <summary>
        ///     Construct a sink posting to the specified database.
        /// </summary>
        /// <param name="databaseUrlOrConnStrName">The URL of a MongoDB database, or connection string name containing the URL.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="collectionName">Name of the MongoDb collection to use for the log. Default is "log".</param>
        /// <param name="collectionCreationOptions">Collection Creation Options for the log collection creation.</param>
        /// <param name="mongoDBJsonFormatter">Formatter to produce json for MongoDB.</param>
        public MongoDBSink(string databaseUrlOrConnStrName,
                           int batchPostingLimit = DEFAULT_BATCH_POSTING_LIMIT,
                           TimeSpan? period = null,
                           IFormatProvider formatProvider = null,
                           string collectionName = DEFAULT_COLLECTION_NAME,
                           CreateCollectionOptions collectionCreationOptions = null,
                           ITextFormatter mongoDBJsonFormatter = null)
            : this(DatabaseFromMongoUrl(databaseUrlOrConnStrName),
                   collectionName,
                   batchPostingLimit,
                   period,
                   formatProvider,
                   collectionCreationOptions,
                   mongoDBJsonFormatter)
        { }

        /// <summary>
        ///     Construct a sink posting to a specified database.
        /// </summary>
        /// <param name="database">The MongoDB database.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="collectionName">Name of the MongoDb collection to use for the log. Default is "log".</param>
        /// <param name="collectionCreationOptions">Collection Creation Options for the log collection creation.</param>
        /// <param name="mongoDBJsonFormatter">Formatter to produce json for MongoDB.</param>
        public MongoDBSink(IMongoDatabase database,
                           string collectionName = DEFAULT_COLLECTION_NAME,
                           int batchPostingLimit = DEFAULT_BATCH_POSTING_LIMIT,
                           TimeSpan? period = null,
                           IFormatProvider formatProvider = null,
                           CreateCollectionOptions collectionCreationOptions = null,
                           ITextFormatter mongoDBJsonFormatter = null)
            : base(batchPostingLimit, period ?? DefaultPeriod)
        {
            _mongoDatabase = database ?? throw new ArgumentNullException(nameof(database));
            _collectionName = collectionName;
            _mongoDbJsonFormatter = mongoDBJsonFormatter ?? new CustomJsonFormatter();

            _mongoDatabase.VerifyCollectionExists(_collectionName, collectionCreationOptions);
        }

        /// <summary>
        ///     Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <returns></returns>
        protected override Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var docs = events.GenerateBsonDocuments(_mongoDbJsonFormatter);
            return Task.WhenAll(GetLogCollection().InsertManyAsync(docs));
        }

        /// <summary>
        ///     Get the MongoDatabase for a specified database url
        /// </summary>
        /// <param name="databaseUrlOrConnStrName">The URL of a MongoDB database, or connection string name containing the URL.</param>
        /// <returns>The MongoDatabase</returns>
        private static IMongoDatabase DatabaseFromMongoUrl(string databaseUrlOrConnStrName)
        {
            if (string.IsNullOrWhiteSpace(databaseUrlOrConnStrName))
            {
                throw new ArgumentNullException(nameof(databaseUrlOrConnStrName));
            }

            MongoUrl mongoUrl;

#if NET_45
            try
            {
                mongoUrl = MongoUrl.Create(databaseUrlOrConnStrName);
            }
            catch (MongoConfigurationException)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[databaseUrlOrConnStrName];
                if (connectionString == null)
                    throw new KeyNotFoundException($"Invalid database url or connection string key: {databaseUrlOrConnStrName}");

                mongoUrl = MongoUrl.Create(connectionString.ConnectionString);
            }
#else
            mongoUrl = MongoUrl.Create(databaseUrlOrConnStrName);
#endif

            var mongoClient = new MongoClient(mongoUrl);
            return mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        /// <summary>
        ///     Gets the log collection.
        /// </summary>
        /// <returns></returns>
        private IMongoCollection<BsonDocument> GetLogCollection() => _mongoDatabase.GetCollection<BsonDocument>(_collectionName);
    }
}