using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModels;

namespace ATTM_API.Services
{
    public class RegressionRunRecordService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<RegressionRunRecord> _regressionRunRecords;

        public RegressionRunRecordService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _regressionRunRecords = database.GetCollection<RegressionRunRecord>(settings.RegressionRunRecordsCollectionName);
        }

        public List<RegressionRunRecord> Get() =>
            _regressionRunRecords.Find(new BsonDocument()).ToList();
            

        public async Task<RegressionRunRecord> Get(string id) =>
            await _regressionRunRecords.Find<RegressionRunRecord>(regressionRunRecord => regressionRunRecord.Id == id).FirstOrDefaultAsync();
    }
}