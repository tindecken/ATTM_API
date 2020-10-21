using ATTM_API.Helpers;
using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class KeywordService
    {
        private readonly IMongoCollection<Keyword> _keywords;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public KeywordService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _keywords = database.GetCollection<Keyword>(settings.KeywordsCollectionName);
        }

        public List<Keyword> Get() =>
            _keywords.Find(new BsonDocument()).ToList();
            

        public async Task<Keyword> Get(string id) =>
            await _keywords.Find<Keyword>(keyword => keyword.Id == id).FirstOrDefaultAsync();

        public void Refresh() {
            CSharpTestProjectHelper.GetKeywords();
            string text = System.IO.File.ReadAllText(@"c:\temp\keywords.json");
            var bsonDoc = BsonDocument.Parse(text);
            Logger.Debug($"{JsonConvert.SerializeObject(bsonDoc)}");
        }
    }
}