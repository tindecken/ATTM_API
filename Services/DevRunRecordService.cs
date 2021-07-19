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
        private readonly IMongoCollection<TestCase> _testCases;

        public DevRunRecordService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _devRunRecords = database.GetCollection<DevRunRecord>(settings.DevRunRecordsCollectionName);
            _testCases = database.GetCollection<TestCase>(settings.TestCasesCollectionName);
        }

        public List<DevRunRecord> Get() =>
            _devRunRecords.Find(new BsonDocument()).ToList();
            

        public async Task<DevRunRecord> Get(string id) =>
            await _devRunRecords.Find<DevRunRecord>(devRunRecord => devRunRecord.Id == id).FirstOrDefaultAsync();

        public async Task<JObject> GetDevRunRecordsForTestCase(string testCaseId)
        {
            JObject result = new JObject();
            var devRunRecords = await _devRunRecords.Find<DevRunRecord>(d => d.TestCaseId == testCaseId)
                .SortByDescending(d => d.StartAt)
                .ToListAsync();
            result.Add("result", "success");
            result.Add("count", devRunRecords.Count);
            if (devRunRecords.Count == 0)
            {
                result.Add("message", "Not found any run records for test case.");
                result.Add("data", new JArray());
            }
            else
            {
                result.Add("message", $"Found {devRunRecords.Count} run records for test case.");
                JArray data = new JArray();
                foreach (var runRecord in devRunRecords)
                {
                    data.Add(JToken.FromObject(runRecord));
                }
                result.Add("data", data);
            }
            return result;
        }
        public async Task<JObject> GetLastDevRunRecordsForTestCase(string testCaseId)
        {
            JObject result = new JObject();
            var devRunRecord = await _devRunRecords.Find<DevRunRecord>(d => d.TestCaseId == testCaseId)
                .SortByDescending(d => d.StartAt)
                .FirstOrDefaultAsync();
            
            result.Add("result", "success");

            if (devRunRecord == null)
            {
                result.Add("data", null);
                result.Add("message", "No data found");
            }
            else
            {
                result.Add("data", JToken.FromObject(devRunRecord));
                result.Add("message", "Get last dev run record success.");
            }

            return result;
        }
        public async Task<JObject> GetAllDevRunRecordsForTestCase()
        {
            JObject result = new JObject();
            JArray data = new JArray();
            var testCaseIdList = await _devRunRecords.Distinct(f => f.TestCaseId, new BsonDocument()).ToListAsync();
            foreach (var testCaseId in testCaseIdList)
            {
                var devRunRecord = await _devRunRecords.Find<DevRunRecord>(d => d.TestCaseId == testCaseId)
                    .SortByDescending(d => d.StartAt)
                    .FirstOrDefaultAsync();
                if (devRunRecord != null) data.Add(JToken.FromObject(devRunRecord));
            }
            result.Add("count", data.Count);
            result.Add("result", "success");
            result.Add("data", data);
            
            return result;
        }
        
    }
}