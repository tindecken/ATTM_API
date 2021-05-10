using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class RegressionTestService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<RegressionTest> _regressionTests;
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private readonly IMongoCollection<TestCase> _testcases;
        private readonly IMongoCollection<RegressionRunRecord> _regresionRunRecords;

        public RegressionTestService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _regressionTests = database.GetCollection<RegressionTest>(settings.RegressionTestsCollectionName);
            _categories = database.GetCollection<Category>(settings.CategoriesCollectionName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _regresionRunRecords = database.GetCollection<RegressionRunRecord>(settings.RegressionRunRecordsCollectionName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
        }

        public async Task<RegressionTest> Create(RegressionTest regressionTest)
        {
            await _regressionTests.InsertOneAsync(regressionTest);
            return regressionTest;
        }

        public List<RegressionTest> Get() =>
            _regressionTests.Find(new BsonDocument()).ToList();

        public async Task<RegressionTest> Get(string id) =>
            await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == id).FirstOrDefaultAsync();
        public async Task<JObject> CreateRegressionTestFromTestCase(TestCase testCase)
        {
            // Get regression
            JObject result = new JObject();

            // Get TestCase FullName
            var category = await _categories.Find<Category>(cat => cat.Id == testCase.CategoryId).FirstOrDefaultAsync();
            var testsuite = await _testsuites.Find<TestSuite>(ts => ts.Id == testCase.TestSuiteId).FirstOrDefaultAsync();
            var testgroup = await _testgroups.Find<TestGroup>(tg => tg.Id == testCase.TestGroupId).FirstOrDefaultAsync();

            var regressionTest = new RegressionTest
            {
                TestCaseCodeName = testCase.CodeName,
                TestCaseName = testCase.Name,
                TestCaseFullName = $"TestProject.TestCases.{category.Name}.{testsuite.CodeName}.{testCase.CodeName}",
                CategoryName = category.Name,
                TestSuiteFullName = $"{testsuite.CodeName}: {testsuite.Name}",
                TestGroupFullName = $"{testgroup.CodeName}: {testgroup.Name}",
                AnalyzeBy = string.Empty,
                Issue = string.Empty,
                Comment = string.Empty,
                IsHighPriority = false,
                QueueId = testCase.QueueId,
                Owner = testCase.Owner,
                Status = "InQueue",
            };

            await _regressionTests.InsertOneAsync(regressionTest);

            result.Add("result", "success");
            result.Add("message", $"Added {regressionTest.TestCaseCodeName}");
            result.Add("data", JToken.FromObject(regressionTest));
            return result;
        }

        public async Task<JObject> CreateRegressionTestFromTestCaseId(string testCaseId)
        {
            // Get regression
            JObject result = new JObject();

            //get current TestCase
            var currTestCase = await _testcases.Find<TestCase>(tc => tc.Id == testCaseId).FirstOrDefaultAsync();
            if (currTestCase == null)
            {
                result.Add("result", "error");
                result.Add("data", null);
                result.Add("message", $"Not found TestCase with ID: {testCaseId}");
            }

            // Get TestCase FullName
            var category = await _categories.Find<Category>(cat => cat.Id == currTestCase.CategoryId).FirstOrDefaultAsync();
            var testsuite = await _testsuites.Find<TestSuite>(ts => ts.Id == currTestCase.TestSuiteId).FirstOrDefaultAsync();
            var testgroup = await _testgroups.Find<TestGroup>(tg => tg.Id == currTestCase.TestGroupId).FirstOrDefaultAsync();

            var regressionTest = new RegressionTest
            {
                TestCaseCodeName = currTestCase.CodeName,
                TestCaseName = currTestCase.Name,
                TestCaseFullName = $"TestProject.TestCases.{category.Name}.{testsuite.CodeName}.{currTestCase.CodeName}",
                CategoryName = category.Name,
                TestSuiteFullName = $"{testsuite.CodeName}: {testsuite.Name}",
                TestGroupFullName = $"{testgroup.CodeName}: {testgroup.Name}",
                AnalyzeBy = string.Empty,
                Issue = string.Empty,
                Comment = string.Empty,
                IsHighPriority = false,
                WorkItem = currTestCase.WorkItem,
                QueueId = currTestCase.QueueId,
                Owner = currTestCase.Owner,
                Status = "InQueue",
            };

            await _regressionTests.InsertOneAsync(regressionTest);

            result.Add("result", "success");
            result.Add("message", $"Added {regressionTest.TestCaseCodeName}");
            result.Add("data", JToken.FromObject(regressionTest));
            return result;
        }
        public async Task<JObject> GetLastRegressionTestRunResult(string RegressionTestId)
        {
            // Get regression
            JObject result = new JObject();
            var currRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == RegressionTestId).FirstOrDefaultAsync();
            if (currRegressionTest == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found RegressionTest with ID: {RegressionTestId}");
                return result;
            }

            // Get last RegressionRunRecordId
            var lastRegressionRunRecordId = currRegressionTest.RegressionRunRecordIds.LastOrDefault();
            if (string.IsNullOrEmpty(lastRegressionRunRecordId))
            {
                result.Add("result", "success");
                result.Add("data", null);
                result.Add("message", "test has no running history");
                return result;
            }

            var lastRegressionRunRecord = await _regresionRunRecords
                .Find<RegressionRunRecord>(rrr => rrr.Id == lastRegressionRunRecordId).FirstOrDefaultAsync();

            // If not found --> some thing error in database --> delete it lastRegressionRunRecordId in currRegressionTest  
            if (lastRegressionRunRecord == null)
            {
                currRegressionTest.RegressionRunRecordIds.RemoveAll(r => r.Equals(lastRegressionRunRecordId));
                await _regressionTests.ReplaceOneAsync(rt => rt.Id == currRegressionTest.Id, currRegressionTest);
                result.Add("result", "success");
                result.Add("data", null);
                result.Add("message", $"Not found Regression Run Record: {lastRegressionRunRecordId} --> delete it.");
                return result;
            }
            else
            {
                result.Add("result", "success");
                result.Add("data", JToken.FromObject(lastRegressionRunRecord));
                result.Add("message", "test has no running history");
                return result;
            }
        }
    }
}