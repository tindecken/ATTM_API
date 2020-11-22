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
        private readonly IMongoCollection<TestGroup> _testgroups;

        public TestSuiteService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
        }

        public async Task<List<TestSuite>> Get() =>
            await _testsuites.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestSuite> Get(string id) =>
            await _testsuites.Find<TestSuite>(ts => ts.Id == id).FirstOrDefaultAsync();

        public async Task<TestGroup> CreateTestGroup(string tsId, TestGroup tg) {
            try
            {
                var existingTestGroup = await _testgroups.Find<TestGroup>(t => t.tgId == tg.tgId).FirstOrDefaultAsync();
                if (existingTestGroup != null)
                {
                    return null;
                }
                else
                {
                    await _testgroups.InsertOneAsync(tg);
                    var filter = Builders<TestSuite>.Filter.Eq(ts => ts.Id, tsId);
                    var update = Builders<TestSuite>.Update.Push<string>(ts => ts.TestGroups, tg.Id);
                    await _testsuites.FindOneAndUpdateAsync(filter, update);
                    return tg;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}