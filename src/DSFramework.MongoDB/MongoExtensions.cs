using System;
using DSFramework.MongoDB.Configuration;
using DSFramework.MongoDB.Contexts;
using DSFramework.MongoDB.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DSFramework.MongoDB
{
    public static class MongoExtensions
    {
        public static void AddMongoDb(this IServiceCollection services, MongoDbSettings dbSettings)
        {
            var mongoContext = new MongoDbContext(ConfigureMongoDb(dbSettings));
            services.AddSingleton<IMongoDbContext>(mongoContext);
        }

        public static void AddMongoDb(this IServiceCollection services, IConfiguration config, string path = "MongoDb")
        {
            services.AddMongoDb(config.GetSection(path).Get<MongoDbSettings>());
        }

        public static IMongoDatabase ConfigureMongoDb(MongoDbSettings config)
        {
            Initializer.RegisterCommonSerializers();
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(config.ConnectionString));
            clientSettings.WaitQueueSize = 10000;

            var client = new MongoClient(clientSettings);
            return client.GetDatabase(config.DbName);
        }
    }
}