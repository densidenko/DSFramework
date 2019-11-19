using System.Linq;
using System.Reflection;
using DSFramework.Data.MongoDB.Attributes;
using DSFramework.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DSFramework.Data.MongoDB.Contexts
{
    /// <summary>
    ///     The MongoDb context
    /// </summary>
    public class MongoDbContext : IMongoDbContext
    {
        /// <summary>
        ///     The IMongoClient from the official MongoDB driver
        /// </summary>
        public IMongoClient Client { get; }

        /// <summary>
        ///     The IMongoDatabase from the official MongoDB driver
        /// </summary>
        public IMongoDatabase Database { get; }

        /// <summary>
        ///     The constructor of the MongoDbContext, it needs an object implementing <see cref="IMongoDatabase" />.
        /// </summary>
        /// <param name="mongoDatabase">An object implementing IMongoDatabase</param>
        public MongoDbContext(IMongoDatabase mongoDatabase)
        {
            // Avoid legacy UUID representation: use Binary 0x04 subtype.
            InitializeGuidRepresentation();
            Database = mongoDatabase;
            Client = Database.Client;
        }

        /// <summary>
        ///     The constructor of the MongoDbContext, it needs a connection string and a database name.
        /// </summary>
        /// <param name="connectionString">The connections string.</param>
        /// <param name="databaseName">The name of your database.</param>
        public MongoDbContext(string connectionString, string databaseName)
        {
            InitializeGuidRepresentation();
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase(databaseName);
        }

        /// <summary>
        ///     The constructor of the MongoDbContext, it needs a connection string and a database name.
        /// </summary>
        /// <param name="client">The MongoClient.</param>
        /// <param name="databaseName">The name of your database.</param>
        public MongoDbContext(MongoClient client, string databaseName)
        {
            InitializeGuidRepresentation();
            Client = client;
            Database = client.GetDatabase(databaseName);
        }

        /// <summary>
        ///     Returns a collection for a document type. Also handles document types with a partition key.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        /// <param name="partitionKey">The optional value of the partition key.</param>
        public virtual IMongoCollection<TDocument> GetCollection<TDocument>(string partitionKey = null)
            => Database.GetCollection<TDocument>(GetCollectionName<TDocument>(partitionKey));

        /// <summary>
        ///     Drops a collection, use very carefully.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        public virtual void DropCollection<TDocument>(string partitionKey = null)
            => Database.DropCollection(GetCollectionName<TDocument>(partitionKey));

        /// <summary>
        ///     Sets the Guid representation of the MongoDB Driver.
        /// </summary>
        /// <param name="guidRepresentation">The new value of the GuidRepresentation</param>
        public virtual void SetGuidRepresentation(GuidRepresentation guidRepresentation) => MongoDefaults.GuidRepresentation = guidRepresentation;

        /// <summary>
        ///     Extracts the CollectionName attribute from the entity type, if any.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        /// <returns>The name of the collection in which the TDocument is stored.</returns>
        protected virtual string GetAttributeCollectionName<TDocument>()
            => (typeof(TDocument).GetTypeInfo().GetCustomAttributes(typeof(CollectionNameAttribute)).FirstOrDefault() as CollectionNameAttribute)
                ?.Name;

        /// <summary>
        ///     Initialize the Guid representation of the MongoDB Driver.
        ///     Override this method to change the default GuidRepresentation.
        /// </summary>
        protected virtual void InitializeGuidRepresentation() => MongoDefaults.GuidRepresentation = GuidRepresentation.Standard;

        /// <summary>
        ///     Given the document type and the partition key, returns the name of the collection it belongs to.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        /// <param name="partitionKey">The value of the partition key.</param>
        /// <returns>The name of the collection.</returns>
        protected virtual string GetCollectionName<TDocument>(string partitionKey)
        {
            var collectionName = GetAttributeCollectionName<TDocument>() ?? Pluralize<TDocument>();
            if (string.IsNullOrEmpty(partitionKey))
            {
                return collectionName;
            }

            return $"{partitionKey}-{collectionName}";
        }

        /// <summary>
        ///     Very naively pluralizes a TDocument type name.
        /// </summary>
        /// <typeparam name="TDocument">The type representing a Document.</typeparam>
        /// <returns>The pluralized document name.</returns>
        protected virtual string Pluralize<TDocument>() => typeof(TDocument).Name.Pluralize().Camelize();
    }
}