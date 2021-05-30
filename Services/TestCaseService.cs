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
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));

        public TestCaseService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
            _categories = database.GetCollection<Category>(settings.CategoriesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
        }

        public async Task<List<TestCase>> Get() =>
            await _testcases.Find(new BsonDocument()).ToListAsync();

        public async Task<JObject> GetAllDetail()
        {
            JObject result = new JObject();
            JArray arrResult = new JArray();
            var testCases = await _testcases.Find(new BsonDocument()).ToListAsync();
            foreach (var testCase in testCases)
            {
                if (testCase.IsDeleted || testCase.IsDisabled) continue;
                JObject temp = new JObject();
                var cat = await _categories.Find<Category>(cat => cat.Id == testCase.CategoryId).FirstOrDefaultAsync();
                var ts = await _testsuites.Find<TestSuite>(s => s.Id == testCase.TestSuiteId).FirstOrDefaultAsync();
                var tg = await _testgroups.Find<TestGroup>(g => g.Id == testCase.TestGroupId).FirstOrDefaultAsync();
                temp.Add("Id", testCase.Id);
                temp.Add("Name", testCase.Name);
                temp.Add("CodeName", testCase.CodeName);
                temp.Add("FullName", $"{testCase.CodeName} - {testCase.Name}");
                temp.Add("Description", testCase.Description);
                temp.Add("Category", cat.Name);
                temp.Add("TestSuite", $"{ts.CodeName} - {ts.Name}");
                temp.Add("TestGroup", $"{tg.CodeName} - {tg.Name}");
                temp.Add("TestCaseType", testCase.TestCaseType);
                temp.Add("IsPrimary", testCase.IsPrimary);
                temp.Add("Owner", testCase.Owner);
                temp.Add("CreatedDate", testCase.CreatedDate);
                temp.Add("LastModifiedDate", testCase.LastModifiedDate);
                temp.Add("Team", testCase.Team);
                temp.Add("Queue", testCase.Queue);
                temp.Add("CategoryId", cat.Id);
                temp.Add("TestSuiteId", ts.Id);
                temp.Add("TestGroupId", tg.Id);
                arrResult.Add(temp);
            }

            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("data", arrResult);
            result.Add("message", null);

            return result;
        }
            
        public async Task<TestCase> Get(string id) =>
            await _testcases.Find<TestCase>(tc => tc.Id == id).FirstOrDefaultAsync();
        public async Task<JObject> SaveTestCaseAsync(TestCase tc)
        {
            Logger.Debug("-- Save Test Case --");
            JObject result = new JObject();
            Logger.Debug($"TestCase: {tc}");
            var filter = Builders<TestCase>.Filter.Eq("_id", ObjectId.Parse(tc.Id));
            var test = _testcases.Find(filter).FirstOrDefault();
            if (test == null) throw new Exception($"Can't find test with id {tc.Id}");
            await _testcases.ReplaceOneAsync(filter, tc);
            result.Add("message", "Saved test !");
            Logger.Debug("-- END Save Test Case --");
            return result;
        }
    }
}