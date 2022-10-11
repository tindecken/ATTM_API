using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using CommonModels;

namespace ATTM_API.Services
{
    public class RegressionTestService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<Regression> _regressions;
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
            _regressions = database.GetCollection<Regression>(settings.RegressionsCollectionName);
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
                TestCaseFullCodeName = $"TestProject.TestCases.{category.Name}.{testsuite.CodeName}.{testCase.CodeName}",
                TestCaseType = testCase.TestCaseType,
                Description = testCase.Description,
                Team = testCase.Team,
                CategoryName = category.Name,
                TestSuiteFullName = $"{testsuite.CodeName}: {testsuite.Name}",
                TestGroupFullName = $"{testgroup.CodeName}: {testgroup.Name}",
                AnalyseBy = string.Empty,
                Issue = string.Empty,
                Comments = string.Empty,
                IsHighPriority = false,
                Queue = testCase.Queue,
                DontRunWithQueues = testCase.DontRunWithQueues,
                Owner = testCase.Owner,
                Status = TestStatus.InQueue
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
        public async Task<JObject> SaveComments(string RegressionTestId, JObject CommentObject)
        {
            var CommentBy = CommentObject["CommentBy"];
            var Comment = CommentObject["Comment"];

            // Get regression test
            JObject result = new JObject();
            var currRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == RegressionTestId).FirstOrDefaultAsync();
            if (currRegressionTest == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found RegressionTest with ID: {RegressionTestId}");
                return result;
            }

            var updatedComments = $"{currRegressionTest.Comments}\r\n{DateTime.UtcNow.ToString("yyyy MMM dd - HH:mm")} - {CommentBy}: {Comment}";
            var update = Builders<RegressionTest>.Update.Set(regTest => regTest.Comments, updatedComments);
            var updated = await _regressionTests.UpdateOneAsync(rt => rt.Id == RegressionTestId, update);
            if (updated.IsAcknowledged)
            {
                var updatedRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == RegressionTestId).FirstOrDefaultAsync();
                result.Add("result", "success");
                result.Add("data", JToken.FromObject(updatedRegressionTest));
                result.Add("message", $"Updated comments for Regression Test: {currRegressionTest.TestCaseName}");
                return result;
            }
            else
            {
                result.Add("result", "error");
                result.Add("data", null);
                result.Add("message", $"Failed to update comments for Regression Test: {currRegressionTest.TestCaseName}");
                return result;
            }
        }

        public async Task<JObject> AddComment(AddCommentData commentData)
        {
            // Get regression test
            JObject result = new JObject();
            JArray arrResult = new JArray();
            if (commentData.RegressionTestIds.Count == 0)
            {
                result.Add("result", "error");
                result.Add("message", $"No Regression Test is provided");
                return result;
            }

            foreach (var regTestId in commentData.RegressionTestIds)
            {
                var currRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == regTestId).FirstOrDefaultAsync();
                if (currRegressionTest == null)
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found RegressionTest with ID: {regTestId}");
                    return result;
                }

                var updatedComments = $"{currRegressionTest.Comments}\r\n{DateTime.UtcNow.ToString("yyyy MMM dd - HH:mm")} - {commentData.CommentBy}: {commentData.Comment}";
                var update = Builders<RegressionTest>.Update.Set(regTest => regTest.Comments, updatedComments);
                var updated = await _regressionTests.UpdateOneAsync(rt => rt.Id == regTestId, update);
                if (updated.IsAcknowledged)
                {
                    var updatedRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == regTestId).FirstOrDefaultAsync();
                    JObject jObject = new JObject();
                    jObject["TestCase"] = updatedRegressionTest.TestCaseName;
                    jObject["Id"] = updatedRegressionTest.Id;
                    jObject["Comments"] = updatedRegressionTest.Comments;
                    arrResult.Add(jObject);
                }
                else
                {
                    result.Add("result", "error");
                    result.Add("data", null);
                    result.Add("message", $"Failed to update comments for Regression Test: {currRegressionTest.TestCaseName}");
                    return result;
                }
            }
            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("data", arrResult);
            result.Add("message", $"Add Comment for {arrResult.Count} test(s) successful.");
            return result;
        }

        public async Task<JObject> setRegressionQueue(SetRegressionQueueData setRegressionQueueData)
        {
            // Get regression test
            JObject result = new JObject();
            JArray arrResult = new JArray();
            if (setRegressionQueueData.RegressionTestIds.Count == 0)
            {
                result.Add("result", "error");
                result.Add("message", $"No Regression Test is provided");
                return result;
            }

            foreach (var regTestId in setRegressionQueueData.RegressionTestIds)
            {
                var currRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == regTestId).FirstOrDefaultAsync();
                if (currRegressionTest == null)
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found RegressionTest with ID: {regTestId}");
                    return result;
                }

                var updatedComments = $"{currRegressionTest.Comments}\r\n{DateTime.UtcNow.ToString("yyyy MMM dd - HH:mm")} - {setRegressionQueueData.UpdateBy}: Re-run on Client: {setRegressionQueueData.ClientName}, HighPriority: {setRegressionQueueData.IsHighPriority}";
                var update = Builders<RegressionTest>.Update.Set(regTest => regTest.Status, TestStatus.InQueue)
                    .Set(regTest => regTest.IsHighPriority, setRegressionQueueData.IsHighPriority)
                    .Set(regTest => regTest.ClientName, setRegressionQueueData.ClientName)
                    .Set(regTest => regTest.Comments, updatedComments);
                var updated = await _regressionTests.UpdateOneAsync(rt => rt.Id == regTestId, update);
                if (updated.IsAcknowledged)
                {
                    var updatedRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == regTestId).FirstOrDefaultAsync();
                    JObject jObject = new JObject();
                    jObject["TestCase"] = updatedRegressionTest.TestCaseName;
                    jObject["Id"] = updatedRegressionTest.Id;
                    jObject["Status"] = updatedRegressionTest.Status.ToString();
                    jObject["IsHighPriority"] = updatedRegressionTest.IsHighPriority;
                    jObject["ClientName"] = updatedRegressionTest.ClientName;
                    jObject["Comments"] = updatedRegressionTest.Comments;
                    arrResult.Add(jObject);
                }
                else
                {
                    result.Add("result", "error");
                    result.Add("data", null);
                    result.Add("message", $"Failed to update queue for Regression Test: {currRegressionTest.TestCaseName}");
                    return result;
                }
            }
            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("data", arrResult);
            result.Add("message", $"Update queue for {arrResult.Count} test(s) successful.");
            return result;
        }

        public async Task<JObject> setRegressionAnalyseStatus(SetRegressionAnalyseStatusData regressionAnalyseStatus)
        {
            // Get regression test
            JObject result = new JObject();
            JArray arrResult = new JArray();
            if (regressionAnalyseStatus.RegressionTestIds.Count == 0)
            {
                result.Add("result", "error");
                result.Add("message", $"No Regression Test is provided");
                return result;
            }

            foreach (var regTestId in regressionAnalyseStatus.RegressionTestIds)
            {
                var currRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == regTestId).FirstOrDefaultAsync();
                if (currRegressionTest == null)
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found RegressionTest with ID: {regTestId}");
                    return result;
                }

                var updatedComments = $"{currRegressionTest.Comments}\r\n{DateTime.UtcNow.ToString("yyyy MMM dd - HH:mm")} - {regressionAnalyseStatus.AnalyseBy}: Set status: {regressionAnalyseStatus.Status}, reason: {regressionAnalyseStatus.Reason}, issue: {regressionAnalyseStatus.Issue}";
                var update = Builders<RegressionTest>.Update.Set(regTest => regTest.Status, regressionAnalyseStatus.Status)
                    .Set(regTest => regTest.AnalyseBy, regressionAnalyseStatus.AnalyseBy)
                    .Set(regTest => regTest.Comments, updatedComments);
                var updated = await _regressionTests.UpdateOneAsync(rt => rt.Id == regTestId, update);
                if (updated.IsAcknowledged)
                {
                    var updatedRegressionTest = await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == regTestId).FirstOrDefaultAsync();
                    JObject jObject = new JObject();
                    jObject["TestCase"] = updatedRegressionTest.TestCaseName;
                    jObject["Id"] = updatedRegressionTest.Id;
                    jObject["Status"] = updatedRegressionTest.Status.ToString();
                    jObject["Reason"] = regressionAnalyseStatus.Reason;
                    jObject["Issue"] = regressionAnalyseStatus.Issue;
                    jObject["Comments"] = updatedRegressionTest.Comments;
                    arrResult.Add(jObject);
                }
                else
                {
                    result.Add("result", "error");
                    result.Add("data", null);
                    result.Add("message", $"Failed to set status for Regression Test: {currRegressionTest.TestCaseName}");
                    return result;
                }
            }
            result.Add("result", "success");
            result.Add("count", arrResult.Count);
            result.Add("data", arrResult);
            result.Add("message", $"Update status for {arrResult.Count} test(s) successful.");
            return result;
        }
    }
}