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

namespace ATTM_API.Services
{
    public class KeywordService
    {
        private readonly IMongoCollection<Keyword> _keywords;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IATTMAppSettings _appSettings;
        public KeywordService(IATTMDatabaseSettings dbSettings, IATTMAppSettings appSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            _keywords = database.GetCollection<Keyword>(dbSettings.KeywordsCollectionName);
            _appSettings = appSettings;
        }

        public List<Keyword> Get() =>
            _keywords.Find(new BsonDocument()).ToList();

        public async Task<JObject> GetKeywords()
        {
            var testProjectHelper = new TestProjectHelper(_appSettings);
            return await testProjectHelper.GetKeywordsJson();
        }
    }
}