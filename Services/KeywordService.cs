using ATTM_API.Helpers;
using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonModels;
using ATTM_API.Models.Entities;

namespace ATTM_API.Services
{
    public class KeywordService
    {
        private readonly IMongoCollection<Keyword> _keywords;
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testSuites;
        private readonly IMongoCollection<TestGroup> _testGroups;
        private readonly IMongoCollection<TestCase> _testCases;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IATTMAppSettings _appSettings;
        public KeywordService(IATTMDatabaseSettings dbSettings, IATTMAppSettings appSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            _keywords = database.GetCollection<Keyword>(dbSettings.KeywordsCollectionName);
            _categories = database.GetCollection<Category>(dbSettings.CategoriesCollectionName);
            _testSuites = database.GetCollection<TestSuite>(dbSettings.TestSuitesCollectionName);
            _testGroups = database.GetCollection<TestGroup>(dbSettings.TestGroupsCollectionName);
            _testCases = database.GetCollection<TestCase>(dbSettings.TestCasesCollectionName);
            _appSettings = appSettings;
        }

        public List<Keyword> Get() =>
            _keywords.Find(new BsonDocument()).ToList();

        public async Task<JObject> GetKeywords()
        {
            var testProjectHelper = new TestProjectHelper(_appSettings);
            return await testProjectHelper.GetKeywordsJson();
        }
        public async Task<ResponseData> GetKeywordCode(CategoryFeatureKeywordData keywordInfo) {
            var testProjectHelper = new TestProjectHelper(_appSettings);
            return await testProjectHelper.GetKeywordCode(keywordInfo);

        }
        public async Task<ResponseData> GetKeywordUsage(CategoryFeatureKeywordData keywordInfo) {
            ResponseData responseData = new ResponseData();
            
            return responseData;

        }
    }
}