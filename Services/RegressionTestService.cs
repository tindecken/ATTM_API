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
    public class RegressionTestService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<RegressionTest> _regressionTests;

        public RegressionTestService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _regressionTests = database.GetCollection<RegressionTest>(settings.RegressionTestsCollectionName);
        }

        public List<RegressionTest> Get() =>
            _regressionTests.Find(new BsonDocument()).ToList();
            

        public async Task<RegressionTest> Get(string id) =>
            await _regressionTests.Find<RegressionTest>(regressionTest => regressionTest.Id == id).FirstOrDefaultAsync();
    }
}