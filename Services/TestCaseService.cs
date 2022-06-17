using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ATTM_API.Services
{
    public class TestCaseService
    {
        private readonly IMongoCollection<TestCase> _testcases;
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<RegressionTest> _regressionTests;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private readonly IMongoCollection<Regression> _regressions;
        private readonly IMongoCollection<TestCaseHistory> _testcasehistories;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));

        public TestCaseService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
            _categories = database.GetCollection<Category>(settings.CategoriesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
            _regressionTests = database.GetCollection<RegressionTest>(settings.RegressionTestsCollectionName);
            _testcasehistories = database.GetCollection<TestCaseHistory>(settings.TestCaseHistoriesCollectionName);
            _regressions = database.GetCollection<Regression>(settings.RegressionsCollectionName);
        }

        public async Task<List<TestCase>> Get() =>
            await _testcases.Find(new BsonDocument()).ToListAsync();

        public async Task<JObject> GetAllDetail()
        {
            JObject result = new JObject();
            JArray arrResult = new JArray();
            var testCases = await _testcases.Find(tc => tc.IsDeleted == false).ToListAsync();
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
                temp.Add("DontRunWithQueues", string.Join(",", testCase.DontRunWithQueues));
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
        
        public async Task<JObject> SaveTestCaseAsync(TestCaseHistory testCaseHistory)
        {
            // Save test case
            JObject result = new JObject();
            TestCase tc = testCaseHistory.TestCase;
            tc.LastModifiedDate = DateTime.Now;
            testCaseHistory.UpdateTestCaseData.UpdateDate = DateTime.UtcNow;
            var filter = Builders<TestCase>.Filter.Eq("_id", ObjectId.Parse(tc.Id));
            var test = _testcases.Find(filter).FirstOrDefault();
            if (test == null)
            {
                result.Add("message", $"Can't find test case with id {tc.Id}");
                result.Add("result", "error");
                return result;
            }
            await _testcases.ReplaceOneAsync(filter, tc);

            // Create new record in collection TestCaseHistory
            await _testcasehistories.InsertOneAsync(testCaseHistory);
            result.Add("message", "Updated test !");
            result.Add("result", "success");
            result.Add("data", $"{testCaseHistory.TestCase}");
            return result;
        }
        public async Task<JObject> UpdateTestCaseAsync(TestCaseHistory testCaseHistory)
        {
            // Update test case
            JObject result = new JObject();
            TestCase tc = testCaseHistory.TestCase;
            UpdateTestCaseData updateData = testCaseHistory.UpdateTestCaseData;
            tc.LastModifiedDate = DateTime.UtcNow;
            tc.LastModifiedUser = updateData.UpdateBy;
            tc.lastModifiedMessage = updateData.UpdateMessage;
            testCaseHistory.UpdateTestCaseData.UpdateDate = DateTime.UtcNow;
            var filterDuplicate = Builders<TestCase>.Filter.Eq("CodeName", tc.CodeName);
            filterDuplicate &= Builders<TestCase>.Filter.Ne("_id", ObjectId.Parse(tc.Id));
            var duplicatedTC = await _testcases.Find(filterDuplicate).FirstOrDefaultAsync();
            if (duplicatedTC != null)
            {
                result.Add("message", $"Duplicated test case code name: {tc.CodeName}");
                result.Add("result", "error");
                return result;
            }

            var filter = Builders<TestCase>.Filter.Eq("_id", ObjectId.Parse(tc.Id));
            var test = _testcases.Find(filter).FirstOrDefault();
            if (test == null)
            {
                result.Add("message", $"Can't find test case with id {tc.Id}");
                result.Add("result", "error");
                return result;
            }
            await _testcases.ReplaceOneAsync(filter, tc);

            // Create new record in collection TestCaseHistory
            await _testcasehistories.InsertOneAsync(testCaseHistory);

            result.Add("message", "Updated test !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(tc));
            return result;
        }
        public async Task<JObject> GetUpdateHistories(string testCaseId)
        {
            JObject result = new JObject();
            var filter = Builders<TestCaseHistory>.Filter.Eq(tc => tc.TestCase.Id, testCaseId);
            var sort = Builders<TestCaseHistory>.Sort.Descending(tc => tc.UpdateTestCaseData.UpdateDate);
            var testCaseHistories = await _testcasehistories.Find(filter).Sort(sort).ToListAsync();
            if (testCaseHistories == null)
            {
                result.Add("data", "No histories!");
                result.Add("result", "success");
                result.Add("count", 0);
            }
            else
            {
                result.Add("data", JToken.FromObject(testCaseHistories));
                result.Add("result", "success");
                result.Add("count", testCaseHistories.Count);
            }

            return result;
        }
        public async Task<JObject> GetLastRegressionResult(string testCaseId)
        {
            JObject result = new JObject();
            var filter = Builders<RegressionTest>.Filter.Eq(rt => rt.TestCaseId, testCaseId);
            var sort = Builders<RegressionTest>.Sort.Descending(rt => rt.Id);
            var lastRegressionTest = await _regressionTests.Find(filter).Sort(sort).FirstOrDefaultAsync();
            if (lastRegressionTest != null)
            {
                var filterRegression = Builders<Regression>.Filter.Eq(r => r.Id, lastRegressionTest.RegressionId);
                filterRegression &= Builders<Regression>.Filter.Eq(r => r.IsDeleted, false);
                var regression = _regressions.Find(filterRegression).FirstOrDefault();
                if (regression != null)
                {
                    var data = new JObject();
                    data.Add("Status", lastRegressionTest.Status);
                    data.Add("Regression", regression.Name);
                    data.Add("Build", regression.Build);
                    data.Add("Issue", lastRegressionTest.Issue);
                    
                    result.Add("data", data);
                    result.Add("result", "success");
                }
                else
                {
                    var data = new JObject();
                    data.Add("Status", lastRegressionTest.Status);
                    
                    result.Add("data", data);
                    result.Add("result", "success");
                }
            }
            else
            {
                result.Add("data", null);
                result.Add("result", "success");
                result.Add("message", "There's no regression test for this test case !");
            }

            return result;
        }

        public async Task<JObject> RestoreTestCase(RestoreTestCaseData restoreTCData)
        {
            JObject result = new JObject();

            var filterTestHistory = Builders<TestCaseHistory>.Filter.Eq("_id", ObjectId.Parse(restoreTCData.Id));
            var testHistory = await _testcasehistories.Find(filterTestHistory).FirstOrDefaultAsync();
            if (testHistory == null)
            {
                result.Add("message", $"Can't find test case history with id: {restoreTCData.Id}");
                result.Add("result", "error");
                return result;
            }

            var restoreTestCase = testHistory.TestCase;
            restoreTestCase.lastModifiedMessage = restoreTCData.RestoreMessage;
            restoreTestCase.LastModifiedDate = DateTime.UtcNow;
            restoreTestCase.LastModifiedUser = restoreTCData.RestoreBy;

            var builderTC = Builders<TestCase>.Filter;
            var filterTC = builderTC.Eq(x => x.Id, restoreTestCase.Id) & builderTC.Eq(x => x.IsDeleted, false);
            var testCase = await _testcases.Find(filterTC).FirstOrDefaultAsync();
            if (testCase == null)
            {
                result.Add("message", $"Test case with id: {restoreTestCase.Id} isn't exists!");
                result.Add("result", "error");
                return result;
            }
            // Update testcase from testCaseHistory
            await _testcases.ReplaceOneAsync(filterTC, restoreTestCase);            

            // Create new test History
            var newTestHistory = new TestCaseHistory();
            newTestHistory.TestCase = restoreTestCase;
            var UpdateData = new UpdateTestCaseData()
            {
                UpdateType = "Restore",
                UpdateBy = restoreTCData.RestoreBy,
                UpdateMessage = $"System: restore from historyId: {restoreTCData.Id}\nUser: {restoreTCData.RestoreMessage}",
                UpdateDate = DateTime.UtcNow
            };
            newTestHistory.UpdateTestCaseData = UpdateData;
            await _testcasehistories.InsertOneAsync(newTestHistory);
            

            result.Add("message", "Restored test !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(restoreTestCase));
            return result;
        }
    }
}