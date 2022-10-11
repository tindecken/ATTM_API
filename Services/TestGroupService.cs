using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using CommonModels;
using Newtonsoft.Json.Linq;

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
                var existingTestCase = await _testcases.Find<TestCase>(t => t.CodeName == tc.CodeName).FirstOrDefaultAsync();
                if (existingTestCase != null) return null;
                await _testcases.InsertOneAsync(tc);
                var filter = Builders<TestGroup>.Filter.Eq(tg => tg.Id, tgId);
                var update = Builders<TestGroup>.Update.Push<string>(tg => tg.TestCaseIds, tc.Id);
                await _testgroups.FindOneAndUpdateAsync(filter, update);
                return tc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<JObject> DeleteTestCases(string testGroupId, List<string> lstTestCaseIds)
        {
            // Get regression test
            JObject result = new JObject();
            int count = 0;
            var currentTestGroup = await _testgroups.Find<TestGroup>(tg => tg.Id == testGroupId).FirstOrDefaultAsync();

            if (currentTestGroup == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found TestGroup with ID: {testGroupId}");
                return result;
            }

            foreach (var testCaseId in lstTestCaseIds)
            {
                
                var filterDef = Builders<TestCase>.Filter.Eq(tc => tc.Id, testCaseId);
                var existingTC = await _testcases.Find(filterDef).FirstAsync();
                if (existingTC == null)
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found TestCase ID {testCaseId}");
                    return result;
                }


                var updateDef = Builders<TestCase>.Update
                    .Set(tc => tc.IsDeleted, true)
                    .Set(tc => tc.CodeName, $"{existingTC.CodeName}_deleted")
                    .Set(tc => tc.Name, $"{existingTC.Name}_deleted");
                var deletedTestCase = await _testcases.FindOneAndUpdateAsync(filterDef, updateDef);
                count++;
                // Update testGroup, remove testCase
                int index = currentTestGroup.TestCaseIds.IndexOf(testCaseId);
                if (index >= 0)
                {
                    // This is import when using PullFilter
                    var remoteTestCaseId = new List<string> { testCaseId };
                    // currentTestGroup.TestCaseIds.RemoveAt(index);
                    var filter = Builders<TestGroup>.Filter.Eq(tg => tg.Id, testGroupId);

                    //Testing purpose: success
                    var testGroupUpdate = Builders<TestGroup>.Update.PullFilter(tg => tg.TestCaseIds,
                        s => remoteTestCaseId.Contains(s));

                    //Testing purpose: error
                    //var testGroupUpdate = Builders<TestGroup>.Update.PullFilter(tg => tg.TestCaseIds,
                    //    s => testCaseId.Contains(s));

                    await _testgroups.FindOneAndUpdateAsync(filter, testGroupUpdate);
                }
                else
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found TestCase ID {testCaseId} in TestGroup {currentTestGroup.Name}");
                    return result;
                }
            }

            result.Add("result", "success");
            result.Add("count", count);
            result.Add("data", null);
            result.Add("message", $"Delete {count} test(s) successful.");
            return result;
        }
        public async Task<JObject> UpdateTestGroupAsync(TestGroup testGroup)
        {
            // Update test group
            JObject result = new JObject();
            var filterDuplicated = Builders<TestGroup>.Filter.Ne("_id", ObjectId.Parse(testGroup.Id));
            filterDuplicated &= Builders<TestGroup>.Filter.Eq("CodeName", testGroup.CodeName);
            filterDuplicated &= Builders<TestGroup>.Filter.Eq("TestSuiteId", testGroup.TestSuiteId);
            var duplicatedTG = await _testgroups.Find(filterDuplicated).FirstOrDefaultAsync();
            if (duplicatedTG != null)
            {
                result.Add("message", $"Duplicated test group code name: {testGroup.CodeName}");
                result.Add("result", "error");
                return result;
            }
            var filter = Builders<TestGroup>.Filter.Eq("_id", ObjectId.Parse(testGroup.Id));
            var ts = _testgroups.Find(filter).FirstOrDefault();
            if (ts == null)
            {
                result.Add("message", $"Can't find test group with id {testGroup.Id}");
                result.Add("result", "error");
                return result;
            }
            var updatedTestGroup = await _testgroups.ReplaceOneAsync(filter, testGroup);

            result.Add("message", "Updated test group !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(testGroup));
            return result;
        }
    }
}