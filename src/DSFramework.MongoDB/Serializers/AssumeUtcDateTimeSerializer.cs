using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DSFramework.MongoDB.Serializers
{
    public class AssumeUtcDateTimeSerializer : DateTimeSerializer
    {
        private const long DATE_TIME_FLAG = 1L;
        private const long TICKS_FLAG = 2L;
        private readonly SerializerHelper _helper;

        private readonly Int64Serializer _int64Serializer = new Int64Serializer();

        public AssumeUtcDateTimeSerializer() : base(DateTimeKind.Utc, BsonType.Document)
        {
            _helper = new SerializerHelper(
                new SerializerHelper.Member("DateTime", DATE_TIME_FLAG),
                new SerializerHelper.Member("Ticks", TICKS_FLAG));
        }

        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            var currentBsonType = bsonReader.GetCurrentBsonType();
            DateTime value;

            switch (currentBsonType)
            {
                case BsonType.Document:
                    value = new DateTime();
                    _helper.DeserializeMembers(
                        context,
                        (elementName, flag) =>
                        {
                            if (flag != DATE_TIME_FLAG)
                            {
                                if (flag != TICKS_FLAG)
                                {
                                    return;
                                }

                                value = new DateTime(_int64Serializer.Deserialize(context), DateTimeKind.Utc);
                            }
                            else
                            {
                                bsonReader.SkipValue();
                            }
                        });
                    break;
                default:
                    throw CreateCannotDeserializeFromBsonTypeException(currentBsonType);
            }

            switch (Kind)
            {
                case DateTimeKind.Local:
                    value = DateTime.SpecifyKind(BsonUtils.ToLocalTime(value), DateTimeKind.Local);
                    break;
                case DateTimeKind.Unspecified:
                    value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
                    break;
                case DateTimeKind.Utc:
                    value = BsonUtils.ToUniversalTime(value);
                    break;
                default:
                    throw CreateCannotDeserializeFromBsonTypeException(currentBsonType);
            }

            return value;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }

            base.Serialize(context, args, value);
        }
    }
}