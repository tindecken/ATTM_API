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
    public class TestClientService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<TestClient> _testclients;

        public TestClientService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _testclients = database.GetCollection<TestClient>(settings.TestClientsCollectionName);
        }

        public List<TestClient> Get() =>
            _testclients.Find(new BsonDocument()).ToList();
            

        public async Task<TestClient> Get(string id) =>
            await _testclients.Find<TestClient>(tclient => tclient.Id == id).FirstOrDefaultAsync();

        public async Task<TestClient> Create(TestClient client)
        {
            await _testclients.InsertOneAsync(client);
            return client;
        }
        public void Update(string id, TestClient client) =>
            _testclients.ReplaceOne(tclient => tclient.Id == id, client);

        public void Remove(TestClient testclient) =>
            _testclients.DeleteOne(tclient => tclient.Id == testclient.Id);

        public void Remove(string id) =>
            _testclients.DeleteOne(tclient => tclient.Id == id);
    }
}