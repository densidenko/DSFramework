using MongoDB.Bson.Serialization;
using System;

namespace CWT.Infrastructure.Repository.Mongo.Serializers
{
    public class Initializer
    {
        public static void RegisterCommonSerializers()
        {
            BsonSerializer.RegisterSerializer(typeof(DateTime), new AssumeUtcDateTimeSerializer());
        }
    }
}