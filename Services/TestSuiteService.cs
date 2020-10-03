using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class TestSuiteService
    {
        private readonly IMongoCollection<TestSuite> _testsuites;

        public TestSuiteService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
        }

        public async Task<List<TestSuite>> Get() =>
            await _testsuites.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestSuite> Get(string id) =>
            await _testsuites.Find<TestSuite>(ts => ts.Id == id).FirstOrDefaultAsync();
    }
}