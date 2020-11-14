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
        private readonly IMongoCollection<TestCase> _testcases;

        public TestGroupService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
        }

        public async Task<List<TestGroup>> Get() =>
            await _testgroups.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestGroup> Get(string id) =>
            await _testgroups.Find<TestGroup>(tg => tg.Id == id).FirstOrDefaultAsync();

        public async Task<TestCase> CreateTestCase(string tgId, TestCase tc)
        {
            try
            {
                var existingTestCase = await _testcases.Find<TestCase>(t => t.Name == tc.Name).FirstOrDefaultAsync();
                if (existingTestCase != null)
                {
                    return null;
                }
                else
                {
                    await _testcases.InsertOneAsync(tc);
                    var filter = Builders<TestGroup>.Filter.Eq(tg => tg.Id, tgId);
                    var update = Builders<TestGroup>.Update.Push<string>(tg => tg.TestCases, tc.Id);
                    await _testgroups.FindOneAndUpdateAsync(filter, update);
                    return tc;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}