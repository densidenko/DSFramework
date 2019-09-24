using System;
using MongoDB.Bson.Serialization;

namespace DSFramework.MongoDB.Serializers
{
    public class Initializer
    {
        public static void RegisterCommonSerializers()
        {
            BsonSerializer.RegisterSerializer(typeof(DateTime), new AssumeUtcDateTimeSerializer());
        }
    }
}