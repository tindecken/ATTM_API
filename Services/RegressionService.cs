using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using CommonModels;

namespace ATTM_API.Services
{
    public class RegressionService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<Regression> _regressions;
        private readonly IMongoCollection<RegressionTest> _regressionTests;
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private readonly IMongoCollection<TestCase> _testcases;
        private readonly IMongoCollection<RegressionRunRecord> _regresionRunRecords;

        public RegressionService(IATTMDatabaseSettings settings)
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

        public async Task<JObject> Get()
        {
            JObject result = new JObject();
            var filter = Builders<Regression>.Filter.Ne(r => r.IsDeleted, true);
            var sort = Builders<Regression>.Sort.Descending(r => r.CreatedDate);
            var regressions = await _regressions.Find(filter).Sort(sort).ToListAsync();
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(regressions));
            result.Add("message", $"There're {regressions.Count} regression(s).");
            return result;
        }

        public async Task<Regression> Get(string id) =>
            await _regressions.Find<Regression>(regression => regression.Id == id).FirstOrDefaultAsync();

        public async Task<Regression> Create(Regression regression)
        {
            try
            {
                var existingReg = await _regressions.Find<Regression>(r => r.Name == regression.Name).FirstOrDefaultAsync();
                if (existingReg == null)
                {
                    await _regressions.InsertOneAsync(regression);
                    return regression;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<JObject> CreateRegressionTests(string regressionId, List<string> testCaseIds)
        {
            // Get regression
            JObject result = new JObject();
            JArray arrResult = new JArray();

            var currentRegression = await _regressions.Find<Regression>(r => r.Id == regressionId).FirstOrDefaultAsync();
            if (currentRegression == null)
            {
                result.Add("result", "error");
                result.Add("data", null);
                result.Add("message", $"Not found Regression with ID: {regressionId}");
                return result;
            }

            //check if regressiontest is already exist in regression (based on codeName)
            List<string> lstExistingCodeName = new List<string>();
            foreach (var rtId in currentRegression.RegressionTestIds)
            {
                var currRegressionTest = await _regressionTests.Find<RegressionTest>(rt => rt.Id == rtId).FirstOrDefaultAsync();
                if (currRegressionTest != null)
                {
                    lstExistingCodeName.Add((currRegressionTest.TestCaseCodeName));
                }
                else
                {
                    //something is wrong, correct it by: remove this regressionTestId in currRegression
                    // Find Index and remove
                    var index = currentRegression.RegressionTestIds.FindIndex(r => r.Equals(rtId));
                    currentRegression.RegressionTestIds.RemoveAt(index);
                    // Update document
                    var filter = Builders<Regression>.Filter.Eq(r => r.Id, regressionId);
                    await _regressions.ReplaceOneAsync(filter, currentRegression);
                    break;
                }
            }

            foreach (var testCaseId in testCaseIds)
            {
                //get current TestCase
                var currTestCase = await _testcases.Find<TestCase>(tc => tc.Id == testCaseId).FirstOrDefaultAsync();
                if (currTestCase == null)
                {
                    result.Add("result", "error");
                    result.Add("data", null);
                    result.Add("message", $"Not found TestCase with ID: {testCaseId}");
                    return result;
                }

                if (lstExistingCodeName.Contains(currTestCase.CodeName)) continue;

                // Get TestCase FullName
                var category = await _categories.Find<Category>(cat => cat.Id == currTestCase.CategoryId).FirstOrDefaultAsync();
                var testsuite = await _testsuites.Find<TestSuite>(ts => ts.Id == currTestCase.TestSuiteId).FirstOrDefaultAsync();
                var testgroup = await _testgroups.Find<TestGroup>(tg => tg.Id == currTestCase.TestGroupId).FirstOrDefaultAsync();

                var regressionTest = new RegressionTest
                {
                    RegressionId = regressionId,
                    TestCaseId = currTestCase.Id,
                    TestCaseCodeName = currTestCase.CodeName,
                    TestCaseName = currTestCase.Name,
                    TestCaseFullCodeName = $"TestProject.TestCases.{category.Name}.{testsuite.CodeName}.{currTestCase.CodeName}",
                    TestCaseType =  currTestCase.TestCaseType,
                    Description =  currTestCase.Description,
                    Team = currTestCase.Team,
                    CategoryName = category.Name,
                    TestSuiteFullName = $"{testsuite.CodeName}: {testsuite.Name}",
                    TestGroupFullName = $"{testgroup.CodeName}: {testgroup.Name}",
                    AnalyseBy = string.Empty,
                    Issue = string.Empty,
                    Comments = string.Empty,
                    IsHighPriority = false,
                    WorkItem = currTestCase.WorkItem,
                    Queue = currTestCase.Queue,
                    DontRunWithQueues = currTestCase.DontRunWithQueues,
                    Owner = currTestCase.Owner,
                    Status = TestStatus.InQueue,
                    RegressionRunRecordIds = new List<string>(),
                    TestSteps = currTestCase.TestSteps
                };

                await _regressionTests.InsertOneAsync(regressionTest);

                //Update 
                var filter = Builders<Regression>.Filter.Eq(r => r.Id, regressionId);
                var update = Builders<Regression>.Update.Push(r => r.RegressionTestIds, regressionTest.Id);
                await _regressions.FindOneAndUpdateAsync(filter, update);

                arrResult.Add(JToken.FromObject(regressionTest));
            }

            result.Add("result", "success");
            result.Add("message", $"Success added {arrResult.Count} records");
            result.Add("data", arrResult);
            result.Add("count", arrResult.Count);

            return result;
        }

        public async Task<JObject> AddTestToRegression(string regressionId, string regressionTestId)
        {
            // Get regression
            JObject result = new JObject();
            var currRegression = await _regressions.Find<Regression>(r => r.Id == regressionId).FirstOrDefaultAsync();
            if (currRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with ID: {regressionId}");
                result.Add("data", null);
                return result;
            }

            var currRegressionTest = await _regressionTests.Find<RegressionTest>(rt => rt.Id == regressionTestId).FirstOrDefaultAsync();
            if (currRegressionTest == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found RegressionTest with ID: {regressionTestId}");
                result.Add("data", null);
                return result;
            }

            if (currRegression.RegressionTestIds != null)
            {
                // Already added but with Id --> not delete it.
                if (currRegression.RegressionTestIds.Contains(regressionTestId))
                {
                    result.Add("result", "success");
                    result.Add("message", $"RegressionTestId {regressionTestId} is already existed");
                    result.Add("data", null);
                    return result;
                }

                // get all RegressionTest and compare with input RegressionTest
                bool isExisted = false;
                foreach (var rtId in currRegression.RegressionTestIds)
                {
                    var regTest = await _regressionTests.Find<RegressionTest>(r => r.Id == rtId)
                        .FirstOrDefaultAsync();
                    if (regTest.TestCaseCodeName.Equals(currRegressionTest.TestCaseCodeName)) isExisted = true;
                }

                // Already added but with CodeName --> delete it.
                if (isExisted)
                {
                    if (string.IsNullOrEmpty(currRegressionTest.RegressionId))
                    {
                        await _regressionTests.FindOneAndDeleteAsync(rt => rt.Id == regressionTestId);
                        result.Add("result", "error");
                        result.Add("message", $"RegressionTest {currRegressionTest.TestCaseCodeName} is already existed, deleted it.");
                        result.Add("data", null);
                        return result;
                    }

                    result.Add("result", "success");
                    result.Add("message", $"RegressionTest {currRegressionTest.TestCaseCodeName} is already existed, but added in release: {currRegressionTest.RegressionId}");
                    result.Add("data", null);
                    return result;
                }
            }

            // Add to regression
            var filter = Builders<Regression>.Filter.Eq(r => r.Id, regressionId);
            var update = Builders<Regression>.Update.Push(r => r.RegressionTestIds, regressionTestId);
            await _regressions.FindOneAndUpdateAsync(filter, update);
            
            result.Add("result", "success");
            result.Add("message", $"Added {currRegressionTest.TestCaseCodeName}");
            result.Add("data", JToken.FromObject(currRegressionTest));
            return result;
        }

        public async Task<JObject> GetDetailRegression(string regressionId)
        {
            // Get regression
            JObject result = new JObject();
            var currRegression = await _regressions.Find<Regression>(r => r.Id == regressionId).FirstOrDefaultAsync();
            if (currRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with ID: {regressionId}");
                result.Add("data", null);
                return result;
            }

            JArray arrayRegTest = new JArray();
            // Get all regressionTests
            foreach (var regTestId in currRegression.RegressionTestIds)
            {
                var currRegressionTest = await _regressionTests.Find<RegressionTest>(rt => rt.Id == regTestId)
                    .FirstOrDefaultAsync();
                if (currRegressionTest == null)
                {
                    result.Add("message", $"Not found RegressionTest for ID: {regTestId}");
                }
                else
                {
                    // Get last RegressionRunRecordId
                    var lastRegressionRunRecordId = currRegressionTest.RegressionRunRecordIds.LastOrDefault();
                    if (!string.IsNullOrEmpty(lastRegressionRunRecordId))
                    {
                        //Get RegressionRunRecord
                        var lastRegRunRecord = await _regresionRunRecords
                            .Find<RegressionRunRecord>(rrr => rrr.Id == lastRegressionRunRecordId)
                            .FirstOrDefaultAsync();
                        if (lastRegRunRecord != null)
                        {
                            currRegressionTest.LastRegressionRunRecord = lastRegRunRecord;
                            if (currRegressionTest.Status != TestStatus.AnalyseFailed
                                && currRegressionTest.Status != TestStatus.InQueue
                                && currRegressionTest.Status != TestStatus.AnalysePassed
                                && currRegressionTest.Status != TestStatus.InCompatible)
                                currRegressionTest.Status = lastRegRunRecord.Status;
                        }
                    }
                    arrayRegTest.Add(JToken.FromObject(currRegressionTest));
                }
            }
            result.Add("result", "success");
            result.Add("message", $"{arrayRegTest.Count} regression test(s)");
            result.Add("data",  arrayRegTest);
            return result;
        }
        public async Task<JObject> FindTestCaseByName(FindRegressionTestData findRegressionTestData)
        {
            // Get regression
            JObject result = new JObject();
            var currRegression = await _regressions.Find<Regression>(r => r.Id == findRegressionTestData.RegressionId).FirstOrDefaultAsync();
            if (currRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with ID: {findRegressionTestData.RegressionId}");
                result.Add("data", null);
                return result;
            }

            JArray arrayRegTest = new JArray();
            // Get all regressionTests by name
            var filter = Builders<RegressionTest>.Filter.Eq(rt => rt.RegressionId, findRegressionTestData.RegressionId);
            filter &= Builders<RegressionTest>.Filter.Where(rt => rt.TestCaseCodeName.ToLower().Contains(findRegressionTestData.Name.ToLower().Trim()) 
                                                                  || rt.TestCaseName.ToLower().Contains(findRegressionTestData.Name.ToLower().Trim()));

            var regressionTests = await _regressionTests.Find(filter).ToListAsync();
            foreach (var regressionTest in regressionTests)
            {
                // Get last RegressionRunRecordId
                var lastRegressionRunRecordId = regressionTest.RegressionRunRecordIds.LastOrDefault();
                if (!string.IsNullOrEmpty(lastRegressionRunRecordId))
                {
                    //Get RegressionRunRecord
                    var lastRegRunRecord = await _regresionRunRecords
                        .Find<RegressionRunRecord>(rrr => rrr.Id == lastRegressionRunRecordId)
                        .FirstOrDefaultAsync();
                    if (lastRegRunRecord != null)
                    {
                        regressionTest.LastRegressionRunRecord = lastRegRunRecord;
                        if (regressionTest.Status != TestStatus.AnalyseFailed
                            && regressionTest.Status != TestStatus.InQueue
                            && regressionTest.Status != TestStatus.AnalysePassed
                            && regressionTest.Status != TestStatus.InCompatible)
                            regressionTest.Status = lastRegRunRecord.Status;
                    }
                }
                arrayRegTest.Add(JToken.FromObject(regressionTest));
            }

            result.Add("result", "success");
            result.Add("message", $"{arrayRegTest.Count} regression test(s)");
            result.Add("data", arrayRegTest);
            return result;
        }
        public async Task<JObject> DeleteRegression(DeleteRegressionData deleteRegressionData)
        {
            // Get regression
            JObject result = new JObject();
            var currRegression = await _regressions.Find<Regression>(r => r.Id == deleteRegressionData.RegressionId).FirstOrDefaultAsync();
            if (currRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with ID: {deleteRegressionData.RegressionId}");
                result.Add("data", null);
                return result;
            }

            var filter = Builders<Regression>.Filter.Eq(r => r.Id, deleteRegressionData.RegressionId);
            var update = Builders<Regression>.Update.Set(r => r.IsDeleted, true)
                .Set(r => r.LastUpdatedBy, deleteRegressionData.DeletedBy)
                .Set(r => r.LastUpdatedDate, DateTime.UtcNow);

            var updateResult = await _regressions.UpdateOneAsync(filter, update);

            result.Add("result", "success");
            result.Add("message", $"Deleted regression: {currRegression.Name}");
            result.Add("data", JToken.FromObject(updateResult));
            return result;
        }
        public async Task<JObject> UpdateRegression(Regression updateRegression)
        {
            // Get regression
            JObject result = new JObject();
            var existingName = await _regressions.Find<Regression>(r => r.Name == updateRegression.Name && r.Id != updateRegression.Id).FirstOrDefaultAsync();
            if (existingName != null)
            {
                result.Add("result", "error");
                result.Add("message", $"Duplicated name: {updateRegression.Name}");
                result.Add("data", null);
                return result;
            }

            var existingRegression = await _regressions.Find<Regression>(r => r.Id == updateRegression.Id).FirstOrDefaultAsync();
            if (existingRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with Id: {updateRegression.Id}");
                result.Add("data", null);
                return result;
            }

            updateRegression.LastUpdatedDate = DateTime.UtcNow;
            var filter = Builders<Regression>.Filter.Eq(r => r.Id, updateRegression.Id);
            var updateResult = await _regressions.FindOneAndReplaceAsync(filter, updateRegression);

            result.Add("result", "success");
            result.Add("message", $"Update Regression success !");
            result.Add("data", JToken.FromObject(updateRegression));
            return result;
        }
    }
}