using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class TestGroupService
    {
        private readonly IMongoCollection<TestGroup> _testgroups;

        public TestGroupService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
        }

        public async Task<List<TestGroup>> Get() =>
            await _testgroups.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestGroup> Get(string id) =>
            await _testgroups.Find<TestGroup>(tg => tg.Id == id).FirstOrDefaultAsync();
    }
}