using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ATTM_API.Services
{
    public class TestSuiteService
    {
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private readonly IMongoCollection<TestCase> _testcases;

        public TestSuiteService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
        }

        public async Task<List<TestSuite>> Get() =>
            await _testsuites.Find(new BsonDocument()).ToListAsync();
            
        public async Task<TestSuite> Get(string id) =>
            await _testsuites.Find<TestSuite>(ts => ts.Id == id).FirstOrDefaultAsync();

        public async Task<TestGroup> CreateTestGroup(string tsId, TestGroup tg) {
            try
            {
                var existingTestGroup = await _testgroups.Find<TestGroup>(t => t.CodeName == tg.CodeName).FirstOrDefaultAsync();
                if (existingTestGroup != null) return null;
                await _testgroups.InsertOneAsync(tg);
                var filter = Builders<TestSuite>.Filter.Eq(ts => ts.Id, tsId);
                var update = Builders<TestSuite>.Update.Push<string>(ts => ts.TestGroupIds, tg.Id);
                var a = await _testsuites.FindOneAndUpdateAsync(filter, update);
                return tg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<JObject> DeleteTestGroups(string testSuiteId, List<string> lstTestGroupIds)
        {
            // Get regression test
            JObject result = new JObject();
            JArray arrDeletedTestGroups = new JArray();
            JArray arrDeletedTestCases = new JArray();
            var currentTestSuite = await _testsuites.Find<TestSuite>(ts => ts.Id == testSuiteId).FirstOrDefaultAsync();

            if (currentTestSuite == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found TestSuite with ID: {testSuiteId}");
                return result;
            }

            foreach (var testGroupId in lstTestGroupIds)
            {
                var deletedTestGroup = await _testgroups.FindOneAndDeleteAsync(tg => tg.Id == testGroupId);
                if (deletedTestGroup != null)
                {
                    arrDeletedTestGroups.Add($"{deletedTestGroup.CodeName}: {deletedTestGroup.Name}");
                    // Delete all testCase of testGroup
                    foreach (var testCaseId in deletedTestGroup.TestCaseIds)
                    {
                        var filterDef = Builders<TestCase>.Filter.Eq(tc => tc.Id, testCaseId);
                        var updateDef = Builders<TestCase>.Update.Set(tc => tc.IsDeleted, true);
                        var deletedTestCase = await _testcases.FindOneAndUpdateAsync(filterDef, updateDef);
                        if (deletedTestCase != null)
                        {
                            arrDeletedTestCases.Add($"{deletedTestCase.CodeName}: {deletedTestCase.Name}");
                        }
                    }

                    // Update TestSuite, remove testGroup
                    int index = currentTestSuite.TestGroupIds.IndexOf(testGroupId);
                    if (index >= 0)
                    {
                        currentTestSuite.TestGroupIds.RemoveAt(index);
                        var testSuiteUpdate = Builders<TestSuite>.Update
                            .Set(ts => ts.TestGroupIds, currentTestSuite.TestGroupIds);
                        await _testsuites.FindOneAndUpdateAsync(g => g.Id == currentTestSuite.Id, testSuiteUpdate);
                    }
                    else
                    {
                        result.Add("result", "error");
                        result.Add("message", $"Not Found TestGroup ID {testGroupId} in TestSuite {currentTestSuite.Name}");
                        return result;
                    }
                }
                else
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found TestGroup ID {testGroupId}");
                    return result;
                }
            }

            result.Add("result", "success");
            result.Add("count", arrDeletedTestGroups.Count);
            result.Add("data", null);
            result.Add("message", $"Delete {arrDeletedTestGroups.Count} testGroup(s), {arrDeletedTestCases.Count} testCases(s) successful.");
            result.Add("deletedTestGroups", arrDeletedTestGroups);
            result.Add("deletedTestCases", arrDeletedTestCases);
            return result;
        }
        public async Task<JObject> UpdateTestSuiteAsync(TestSuite testSuite)
        {
            // Update test suite
            JObject result = new JObject();
            var filterDuplicated = Builders<TestSuite>.Filter.Ne("_id", ObjectId.Parse(testSuite.Id));
            filterDuplicated &= Builders<TestSuite>.Filter.Eq("CodeName", testSuite.CodeName);
            filterDuplicated &= Builders<TestSuite>.Filter.Eq("CategoryId", testSuite.CategoryId);
            var duplicatedTS = await _testsuites.Find(filterDuplicated).FirstOrDefaultAsync();
            if (duplicatedTS != null)
            {
                result.Add("message", $"Duplicated test suite code name: {testSuite.CodeName}");
                result.Add("result", "error");
                return result;
            }
            var filter = Builders<TestSuite>.Filter.Eq("_id", ObjectId.Parse(testSuite.Id));
            var ts = _testsuites.Find(filter).FirstOrDefault();
            if (ts == null)
            {
                result.Add("message", $"Can't find test suite with id {testSuite.Id}");
                result.Add("result", "error");
                return result;
            }
            var updatedTestSuite = await _testsuites.ReplaceOneAsync(filter, testSuite);

            result.Add("message", "Updated test suite !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(testSuite));
            return result;
        }
    }
}