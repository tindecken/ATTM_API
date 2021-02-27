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
            try
            {
                var existingTS = await _testsuites.Find<TestSuite>(t => t.tsId == ts.tsId).FirstOrDefaultAsync();
                if (existingTS != null) return null;
                await _testsuites.InsertOneAsync(ts);
                var filter = Builders<Category>.Filter.Eq(cat => cat.Id, catId);
                var update = Builders<Category>.Update.Push<string>(cat => cat.TestSuites, ts.Id);
                await _categories.FindOneAndUpdateAsync(filter, update);
                return ts;
            }
            catch (Exception ex)
            {
                throw ex;   
            }
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
                catObject["nodeType"] = "Category";
                catObject["label"] = cat.Name;
                JArray arrTS = new JArray();
                foreach (var tsId in cat.TestSuites) {
                    JObject tsObject = new JObject();
                    JArray arrTG = new JArray();
                    Logger.Debug($"tsId: {tsId}");
                    var ts = await _testsuites.Find<TestSuite>(ts => ts.Id == tsId).FirstOrDefaultAsync();
                    tsObject = (JObject)JToken.FromObject(ts);
                    tsObject["nodeType"] = "TestSuite";
                    tsObject["label"] = $"{ts.tsId}: {ts.Name}";
                    tsObject["catId"] = $"{cat.Id}";
                    foreach (var tgId in ts.TestGroups)
                    {
                        JObject tgObject = new JObject();
                        JArray arrTC = new JArray();
                        var tg = await _testgroups.Find<TestGroup>(tg => tg.Id == tgId).FirstOrDefaultAsync();
                        tgObject = (JObject)JToken.FromObject(tg);
                        tgObject["nodeType"] = "TestGroup";
                        tgObject["label"] = $"{tg.tgId}: {tg.Name}";
                        tgObject["catId"] = $"{cat.Id}";
                        tgObject["tsId"] = $"{ts.Id}";
                        foreach (var tcId in tg.TestCases)
                        {
                            JObject tcObject = new JObject();
                            var tc = await _testcases.Find<TestCase>(tc => tc.Id == tcId).FirstOrDefaultAsync();
                            tcObject = (JObject)JToken.FromObject(tc);
                            tcObject["nodeType"] = "TestCase";
                            tcObject["label"] = $"{tc.tcId}: {tc.Name}";
                            tsObject["catId"] = $"{cat.Id}";
                            tsObject["tsId"] = $"{ts.Id}";
                            tsObject["tgId"] = $"{tg.Id}";
                            tcObject["TestGroup"] = tg.Name;
                            tcObject["TestSuite"] = ts.Name;
                            tcObject["Category"] = cat.Name;
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
    }
}