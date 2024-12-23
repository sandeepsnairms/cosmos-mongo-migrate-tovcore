using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OnlineMongoMigrationProcessor
{
    internal static class MongoHelper
    {
        public static long GenerateQueryAndCount(IMongoCollection<BsonDocument> _collection, BsonValue? gte, BsonValue? lt, out string queryString)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;

            // Initialize an empty filter
            FilterDefinition<BsonDocument> filter = FilterDefinition<BsonDocument>.Empty;

            // Add conditions based on gte and lt values
            if (!(gte == null || gte.IsBsonNull) && !(lt == null || lt.IsBsonNull))
            {
                filter = filterBuilder.And(
                    filterBuilder.Gte("_id", gte.AsObjectId),
                    filterBuilder.Lte("_id", lt.AsObjectId)
                );
            }
            else if (!(gte == null || gte.IsBsonNull))
            {
                filter = filterBuilder.Gte("_id", gte.AsObjectId);
            }
            else if (!(lt == null || lt.IsBsonNull))
            {
                filter = filterBuilder.Lte("_id", lt.AsObjectId);
            }

            queryString = GenerateQueryString(gte,lt);

            // Execute the query and return the count
            var count = _collection.CountDocuments(filter);
            return count;
        }

        public static string GenerateQueryString(BsonValue? gte, BsonValue? lt)
        {
            // Initialize the query string
            string queryString = "{ \\\"_id\\\": { ";

            // Track the conditions added to ensure correct formatting
            var conditions = new List<string>();

            // Add $gte condition if present
            if (!(gte == null || gte.IsBsonNull))
            {
                conditions.Add($"\\\"$gte\\\": {{\\\"$oid\\\":\\\"{gte}\\\"}}");
            }

            // Add $lte condition if present
            if (!(lt == null || lt.IsBsonNull))   
            {
                conditions.Add($"\\\"$lte\\\": {{\\\"$oid\\\":\\\"{lt}\\\"}}");
            }

            // Combine the conditions with a comma
            queryString += string.Join(", ", conditions);

            // Close the query string
            queryString += " } }";

            return queryString;
        }


        public static BsonValue ConvertToBsonValue(JObject jObject)
        {
            // Create a new BsonDocument
            var bsonDoc = new BsonDocument();

            // Iterate through JObject properties
            foreach (var property in jObject.Properties())
            {
                if (property.Value.Type == JTokenType.Object && property.Value["$oid"] != null)
                {
                    // Convert ObjectId in JObject to BsonObjectId
                    string oidString = property.Value["$oid"].ToString();
                    return new BsonObjectId(ObjectId.Parse(oidString));
                }
                else
                {
                    // Convert regular JObject properties to BsonValues
                   return  BsonValue.Create(property.Value);
                }
            }

            return null;
        }

    }
}
