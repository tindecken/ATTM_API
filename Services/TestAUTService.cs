using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModels;

namespace ATTM_API.Services
{
    public class TestAUTService
    {
        private readonly IMongoCollection<TestAUT> _testAUTs;

        public TestAUTService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testAUTs = database.GetCollection<TestAUT>(settings.TestAUTsCollectionName);
        }

        public async Task<List<TestAUT>> Get() =>
            await _testAUTs.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestAUT> Get(string id) =>
            await _testAUTs.Find<TestAUT>(tAUT => tAUT.Id == id).FirstOrDefaultAsync();
    }
}