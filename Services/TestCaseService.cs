using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class TestCaseService
    {
        private readonly IMongoCollection<TestCase> _testcases;

        public TestCaseService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
        }

        public async Task<List<TestCase>> Get() =>
            await _testcases.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestCase> Get(string id) =>
            await _testcases.Find<TestCase>(tc => tc.Id == id).FirstOrDefaultAsync();
    }
}