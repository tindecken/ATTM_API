using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class KeywordService
    {
        private readonly IMongoCollection<Keyword> _keywords;

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

    }
}