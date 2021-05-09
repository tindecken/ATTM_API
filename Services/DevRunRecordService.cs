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
    public class DevRunRecordService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<DevRunRecord> _devRunRecords;

        public DevRunRecordService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _devRunRecords = database.GetCollection<DevRunRecord>(settings.DevRunRecordsCollectionName);
        }

        public List<DevRunRecord> Get() =>
            _devRunRecords.Find(new BsonDocument()).ToList();
            

        public async Task<DevRunRecord> Get(string id) =>
            await _devRunRecords.Find<DevRunRecord>(devRunRecord => devRunRecord.Id == id).FirstOrDefaultAsync();
    }
}