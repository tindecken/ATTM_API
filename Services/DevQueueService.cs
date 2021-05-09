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
    public class DevQueueService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<DevQueue> _devQueues;

        public DevQueueService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _devQueues = database.GetCollection<DevQueue>(settings.DevQueuesCollectionName);
        }

        public List<DevQueue> Get() =>
            _devQueues.Find(new BsonDocument()).ToList();
            

        public async Task<DevQueue> Get(string id) =>
            await _devQueues.Find<DevQueue>(devQueue => devQueue.Id == id).FirstOrDefaultAsync();
    }
}