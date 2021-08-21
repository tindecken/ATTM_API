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
    public class CategoryService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;
        private readonly IMongoCollection<TestGroup> _testgroups;
        private readonly IMongoCollection<TestCase> _testcases;

        public CategoryService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _categories = database.GetCollection<Category>(settings.CategoriesCollectionName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
            _testgroups = database.GetCollection<TestGroup>(settings.TestGroupsCollectionName);
            _testcases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
        }

        public List<Category> Get() =>
            _categories.Find(new BsonDocument()).ToList();
            

        public async Task<Category> Get(string id) =>
            await _categories.Find<Category>(category => category.Id == id).FirstOrDefaultAsync();

        public async Task<Category> Create(Category category)
        {
            try {
                var existingCat = await _categories.Find<Category>(cat => cat.Name == category.Name).FirstOrDefaultAsync();
                if(existingCat == null) {
                    await _categories.InsertOneAsync(category);
                    return category;
                }else{
                    return null;
                }
            } catch (Exception ex) {
                throw ex;   
            }
        }

        public async Task<TestSuite> CreateTestSuite(string catId, TestSuite ts)
        {
            var existingTS = await _testsuites.Find<TestSuite>(t => t.CodeName == ts.CodeName).FirstOrDefaultAsync();
            if (existingTS != null) return null;
            await _testsuites.InsertOneAsync(ts);
            var filter = Builders<Category>.Filter.Eq(cat => cat.Id, catId);
            var update = Builders<Category>.Update.Push(cat => cat.TestSuiteIds, ts.Id);
            await _categories.FindOneAndUpdateAsync(filter, update);
            return ts;
        }
        public async Task<JObject> GetAllAsync()
        {
            JObject result = new JObject();
            JArray arrResult = new JArray();
            var allCats = await _categories.Find(new BsonDocument()).ToListAsync();
            foreach (var cat in allCats)
            {
                JObject catObject = new JObject();
                catObject = (JObject)JToken.FromObject(cat);
                JArray arrTS = new JArray();
                foreach (var tsId in cat.TestSuiteIds) {
                    JObject tsObject = new JObject();
                    JArray arrTG = new JArray();
                    var ts = await _testsuites.Find<TestSuite>(s => s.Id == tsId).FirstOrDefaultAsync();
                    tsObject = (JObject)JToken.FromObject(ts);
                    foreach (var tgId in ts.TestGroupIds)
                    {
                        JObject tgObject = new JObject();
                        JArray arrTC = new JArray();
                        var tg = await _testgroups.Find<TestGroup>(g => g.Id == tgId).FirstOrDefaultAsync();
                        tgObject = (JObject)JToken.FromObject(tg);
                        foreach (var tcId in tg.TestCaseIds)
                        {
                            JObject tcObject = new JObject();
                            var tc = await _testcases.Find<TestCase>(t => t.Id == tcId).FirstOrDefaultAsync();
                            tcObject = (JObject)JToken.FromObject(tc);
                            arrTC.Add(tcObject);
                        }
                        tgObject["children"] = arrTC;
                        arrTG.Add(tgObject);
                    }
                    tsObject["children"] = arrTG;
                    arrTS.Add(tsObject);
                }
                catObject["children"] = arrTS;
                arrResult.Add(catObject);
            }
            result["result"] = arrResult;
            return result;
        }

        public void Update(string id, Category categoryIn) =>
            _categories.ReplaceOne(category => category.Id == id, categoryIn);

        public void Remove(Category categoryIn) =>
            _categories.DeleteOne(category => category.Id == categoryIn.Id);

        public void Remove(string id) => 
            _categories.DeleteOne(category => category.Id == id);

        public async Task<JObject> DeleteTestSuites(string categoryId, List<string> lstTestSuiteIds)
        {
            // Get regression test
            JObject result = new JObject();
            JArray arrDeleteTestSuite = new JArray();
            JArray arrDeleteTestGroup = new JArray();
            JArray arrDeleteTestCase = new JArray();
            var currentCategory = await _categories.Find<Category>(c => c.Id == categoryId).FirstOrDefaultAsync();

            if (currentCategory == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Category with ID: {categoryId}");
                return result;
            }

            foreach (var testSuiteId in lstTestSuiteIds)
            {
                var deletedTestSuite = await _testsuites.FindOneAndDeleteAsync(ts => ts.Id == testSuiteId);
                if (deletedTestSuite != null)
                {
                    arrDeleteTestSuite.Add($"{deletedTestSuite.CodeName}: {deletedTestSuite.Name}");

                    foreach (var testGroupId in deletedTestSuite.TestGroupIds)
                    {
                        var deleteTestGroup = await _testgroups.FindOneAndDeleteAsync(tg => tg.Id == testGroupId);
                        if (deleteTestGroup != null)
                        {
                            arrDeleteTestGroup.Add($"{deleteTestGroup.CodeName}: {deleteTestGroup.Name}");
                            foreach (var testCaseId in deleteTestGroup.TestCaseIds)
                            {
                                var deleteTestCase = await _testcases.FindOneAndDeleteAsync(tc => tc.Id == testCaseId);
                                if (deleteTestCase != null) arrDeleteTestCase.Add($"{deleteTestCase.CodeName}: {deleteTestCase.Name}");
                            }
                        }
                    }

                    int index = currentCategory.TestSuiteIds.IndexOf(testSuiteId);
                    if (index >= 0)
                    {
                        currentCategory.TestSuiteIds.RemoveAt(index);
                        var categoryUpdate = Builders<Category>.Update
                            .Set(c => c.TestSuiteIds, currentCategory.TestSuiteIds);
                        await _categories.FindOneAndUpdateAsync(c => c.Id == currentCategory.Id, categoryUpdate);
                    }
                    else
                    {
                        result.Add("result", "error");
                        result.Add("message", $"Not Found TestSuite ID {testSuiteId} in Category {currentCategory.Name}");
                        return result;
                    }
                }
                else
                {
                    result.Add("result", "error");
                    result.Add("message", $"Not Found TestSuite ID {testSuiteId}");
                    return result;
                }
            }

            result.Add("result", "success");
            result.Add("count", lstTestSuiteIds.Count);
            result.Add("data", null);
            result.Add("message", $"Delete {arrDeleteTestSuite.Count} testSuite(s), {arrDeleteTestGroup.Count} testGroup(s), {arrDeleteTestCase.Count} testCase(s) successful.");
            result.Add("deleteTestSuites", arrDeleteTestSuite);
            result.Add("deleteTestGroups", arrDeleteTestGroup);
            result.Add("deleteTestCases", arrDeleteTestCase);
            return result;
        }
    }
}