using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineMongoMigrationProcessor
{
    public class SamplePartitioner
    {
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
        public List<(BsonValue Min, BsonValue Max)> CreatePartitions(string partitionKey, int partitionCount)
        {
            if (partitionCount <= 1)
                throw new ArgumentException("Partition count must be greater than 1.");

            // Calculate sampleCount as 10 times the partitionCount
            int sampleCount = partitionCount * 10;

            // Cap sampleCount at 2000
            sampleCount = Math.Min(sampleCount, 2000);

            //reduce partitionCount if less samples
            partitionCount = Math.Min(sampleCount, partitionCount);

            Log.WriteLine($"SampleCount: {sampleCount}, Partition Count: {partitionCount}");
            Log.Save();            

            // Step 1: Sample the data
            var pipeline = new[]
                {
                new BsonDocument("$sample", new BsonDocument("size",sampleCount )), 
                new BsonDocument("$project", new BsonDocument(partitionKey, 1))             // Keep only the partition key
            };

            var sampledData = _collection.Aggregate<BsonDocument>(pipeline).ToList();
            var partitionValues = sampledData
                .Select(doc => doc.GetValue(partitionKey, BsonNull.Value))
                .Where(value => value != BsonNull.Value)
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            Log.WriteLine($"Total Chunks: {partitionValues.Count}");
            Log.Save();

            // Step 2: Calculate partition boundaries
            if (partitionValues.Count < partitionCount)
            {
                throw new InvalidOperationException("Not enough unique values in the partition key to create partitions.");
            }

            var boundaries = new List<(BsonValue Min, BsonValue Max)>();
            int step = partitionValues.Count / partitionCount;

            for (int i = 0; i < partitionCount; i++)
            {
                var min = partitionValues[i * step];
                var max = i == partitionCount - 1 ? BsonMaxKey.Value : partitionValues[(i + 1) * step];
                boundaries.Add((min, max));
            }

            return boundaries;
        }
    }

    
}
