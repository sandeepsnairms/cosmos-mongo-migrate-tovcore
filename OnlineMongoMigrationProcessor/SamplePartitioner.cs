using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Serializers;

namespace OnlineMongoMigrationProcessor
{
    public class SamplePartitioner
    {
#pragma warning disable CS8603
#pragma warning disable CS8604

        private readonly IMongoCollection<BsonDocument> _collection;

        public SamplePartitioner(IMongoCollection<BsonDocument> collection)
        {
            _collection = collection;
        }

        /// <summary>
        /// Creates partitions based on sampled data from the collection.
        /// </summary>
        /// <param name="partitionKey">The field used as the partition key.</param>
        /// <param name="partitionCount">The number of desired partitions.</param>
        /// <returns>A list of partition boundaries.</returns>
        public List<(BsonValue Min, BsonValue Max)> CreatePartitions(string partitionKey, int partitionCount, DataType dataType, long minPartitonSize)
        {
            if (partitionCount <= 1)
                throw new ArgumentException("Partition count must be greater than 1.");

            // Calculate sampleCount as 10 times the partitionCount
            int sampleCount = partitionCount * 10;

            // Cap sampleCount at 2000
            sampleCount = Math.Min(sampleCount, 2000);

            //reduce partitionCount if less samples
            partitionCount = Math.Min(sampleCount, partitionCount);

            

            // Step 1: Build the filter pipeline based on the data type
            long docCountByType= GetDocumentCountByDataType(_collection,partitionKey,dataType);
            if (docCountByType == 0)
            {
                Log.WriteLine($"0 Documents where {partitionKey} is {dataType}");
                Log.Save();
                return null;
            }
            else if (docCountByType < minPartitonSize)
            {
                Log.WriteLine($"Document Count where {partitionKey} is {dataType}:{docCountByType} is less than min partiton size.");
                sampleCount = 1;
                partitionCount = 1;
            }

            Log.WriteLine($"SampleCount: {sampleCount}, Partition Count: {partitionCount} where {partitionKey} is {dataType}");
            Log.Save();


            BsonDocument matchCondition = MatchConditionBuilder(dataType, partitionKey);

            // Step 2: Sample the data
            var pipeline = new[]
            {
                new BsonDocument("$match", matchCondition),  // Add the match condition for the data type
                new BsonDocument("$sample", new BsonDocument("size", sampleCount)),
                new BsonDocument("$project", new BsonDocument(partitionKey, 1)) // Keep only the partition key
            };

            var sampledData = _collection.Aggregate<BsonDocument>(pipeline).ToList();
            var partitionValues = sampledData
                .Select(doc => doc.GetValue(partitionKey, BsonNull.Value))
                .Where(value => value != BsonNull.Value)
                .Distinct()
                .OrderBy(value => value)
                .ToList();

           

            // Step 2: Calculate partition boundaries

            var boundaries = new List<(BsonValue Min, BsonValue Max)>();
            int step = Math.Max(1,partitionValues.Count / partitionCount);

            for (int i = 0; i < partitionCount; i++)
            {
                var min = partitionValues[i * step];
                var max = i == partitionCount - 1 ? BsonMaxKey.Value : partitionValues[(i + 1) * step];
                boundaries.Add((min, max));
            }

            Log.WriteLine($"Total Chunks: {boundaries.Count} where {partitionKey} is {dataType}");
            Log.Save();

            return boundaries;
            
        }

        public static long GetDocumentCountByDataType(IMongoCollection<BsonDocument> collection, string partitionKey, DataType dataType)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;

            BsonDocument matchCondition = MatchConditionBuilder(dataType,partitionKey);

            // Get the count of documents matching the filter
            var count = collection.CountDocuments(matchCondition);
            return count;
        }

        private static BsonDocument MatchConditionBuilder(DataType dataType, string partitionKey)
        {
            BsonDocument matchCondition;
            switch (dataType)
            {
                case DataType.ObjectId:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 7)); // 7 is BSON type for ObjectId
                    break;
                case DataType.Int:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 16)); // 16 is BSON type for Int32
                    break;
                case DataType.Int64:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 18)); // 18 is BSON type for Int64
                    break;
                case DataType.String:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 2)); // 2 is BSON type for String
                    break;
                case DataType.Decimal128:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 19)); // 19 is BSON type for Decimal128
                    break;
                case DataType.Date:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 9)); // 9 is BSON type for Date
                    break;
                case DataType.UUID:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 4)); // 4 is BSON type for Binary (UUID)
                    break;
                case DataType.Object:
                    matchCondition = new BsonDocument(partitionKey, new BsonDocument("$type", 3)); // 3 is BSON type for embedded document (Object)
                    break;
                default:
                    throw new ArgumentException($"Unsupported DataType: {dataType}");
            }
            return matchCondition;
        }
    }


}
