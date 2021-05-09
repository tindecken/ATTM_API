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
    public class RegressionService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<Regression> _regressions;

        public RegressionService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _regressions = database.GetCollection<Regression>(settings.RegressionsCollectionName);
        }

        public List<Regression> Get() =>
            _regressions.Find(new BsonDocument()).ToList();
            

        public async Task<Regression> Get(string id) =>
            await _regressions.Find<Regression>(regression => regression.Id == id).FirstOrDefaultAsync();
    }
}