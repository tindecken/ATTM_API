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

namespace ATTM_API.Services
{
    public class KeywordService
    {
        private readonly IMongoCollection<Keyword> _keywords;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        public static string sKeywordListFile = Path.Combine(Path.GetTempPath(), "Keyword.json");
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

        public async Task<JObject> RefreshAsync() {
            CSharpTestProjectHelper.GetKeywords();
            using (var streamReader = new StreamReader(sKeywordListFile))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    using (var jsonReader = new JsonReader(line))
                    {
                        var context = BsonDeserializationContext.CreateRoot(jsonReader);
                        Logger.Debug($"context {Newtonsoft.Json.JsonConvert.SerializeObject(context)}");
                        Keyword document = _keywords.DocumentSerializer.Deserialize(context);
                        await _keywords.InsertOneAsync(document);
                        Logger.Debug($"{Newtonsoft.Json.JsonConvert.SerializeObject(document)}");
                    }
                }
            }
            JObject kw = JObject.Parse(File.ReadAllText(sKeywordListFile));
            return kw;
        }
    }
}