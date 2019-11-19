using System.Collections.Generic;
using DSFramework.Domain.Abstractions.Repositories;
using MongoDB.Bson;

namespace DSFramework.Data.MongoDB
{
    public class SortBuilder
    {
        public BsonDocument Build(SortOptions sortOptions)
        {
            var sorting = new List<KeyValuePair<string, object>>();

            if (sortOptions.SortByTextScore)
            {
                var name = "score";
                sorting.Add(new KeyValuePair<string, object>(name, new BsonDocument(name: "$meta", value: "textScore")));
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