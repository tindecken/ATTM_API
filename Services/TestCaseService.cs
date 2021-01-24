using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ATTM_API.Services
{
    public class TestCaseService
    {
        private readonly IMongoCollection<TestCase> _testcases;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));

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
        public async Task<JObject> SaveTestCaseAsync(TestCase tc)
        {
            JObject result = new JObject();
            Logger.Debug($"TestCase: {tc}");
            var filter = Builders<TestCase>.Filter.Eq("_id", ObjectId.Parse(tc.Id));
            var test = _testcases.Find(filter).FirstOrDefault();
            if (test == null) throw new Exception($"Can't find test with id {tc.Id}");
            await _testcases.ReplaceOneAsync(filter, tc);
            result.Add("message", "Saved test !");
            return result;
        }
    }
}