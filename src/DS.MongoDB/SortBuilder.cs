using CWT.Infrastructure.Domain.Abstractions.Repositories;
using MongoDB.Bson;
using System.Collections.Generic;

namespace CWT.Infrastructure.Repository.Mongo
{
    public class SortBuilder
    {
        public BsonDocument Build(SortOptions sortOptions)
        {
            var sorting = new List<KeyValuePair<string, object>>();

            if (sortOptions.SortByTextScore)
            {
                var name = "score";
                sorting.Add(new KeyValuePair<string, object>(name, new BsonDocument("$meta", "textScore")));
            }

            foreach (var field in sortOptions.GetFields())
            {
                var name = field.Key;
                var isAscending = field.Value;
                sorting.Add(new KeyValuePair<string, object>(name, isAscending ? 1 : -1));
            }

            return new BsonDocument
            {
                {
                    "$sort",
                    new BsonDocument(sorting)
                }
            };
        }
    }
}