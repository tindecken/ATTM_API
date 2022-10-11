using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using CommonModels;
using Newtonsoft.Json.Linq;

namespace ATTM_API.Services
{
    public class TestEnvService
    {
        private readonly IMongoCollection<TestEnv> _testenvironments;
        private readonly IMongoCollection<TestEnvHistory> _testenvhistories;

        public TestEnvService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _testenvironments = database.GetCollection<TestEnv>(settings.TestEnvironmentsCollectionName);
            _testenvhistories = database.GetCollection<TestEnvHistory>(settings.TestEnvHistoriesCollectionName);
        }

        public List<TestEnv> Get()
        {
            return _testenvironments.Find(te => te.IsDeleted == false).ToList();
        }

        public async Task<TestEnv> Get(string id) =>
            await _testenvironments.Find<TestEnv>(tv => tv.Id == id).FirstOrDefaultAsync();
        
        public async Task<TestEnv> Create(TestEnv testEnv) {
            try {
                var existingEnv = await _testenvironments.Find<TestEnv>(tv => tv.Name == testEnv.Name).FirstOrDefaultAsync();
                if(existingEnv == null) {
                    await _testenvironments.InsertOneAsync(testEnv);
                    return testEnv;
                }else{
                    return null;
                }
            } catch (Exception ex) {
                throw ex;   
            }
        }
        public async Task<JObject> CloneTestEnv(TestEnvCloneData testEnvCloneData)
        {
            // Get regression
            JObject result = new JObject();
            var originalTestEnv = await _testenvironments.Find<TestEnv>(tv => tv.Id == testEnvCloneData.Id).FirstOrDefaultAsync();
            var existingTestEnv = await _testenvironments.Find<TestEnv>(tv => tv.Name == testEnvCloneData.NewName).FirstOrDefaultAsync();
            if (originalTestEnv == null)
            {
                result.Add("result", "error");
                result.Add("message", $"There's no Test Environment for id: {testEnvCloneData.Id}");
                return result;
            }
            if (existingTestEnv != null)
            {
                result.Add("result", "error");
                result.Add("message", $"The Test Environment Name: {testEnvCloneData.NewName} is taken, choose another one.");
                return result;
            }

            var cloneEnv = new TestEnv
            {
                Name = testEnvCloneData.NewName,
                Description = testEnvCloneData.NewDescription,
                Nodes = originalTestEnv.Nodes,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedUser = testEnvCloneData.CloneBy,
                LastModifiedMessage = $"Clone from Test Env: {originalTestEnv.Name}$"
            };

            await _testenvironments.InsertOneAsync(cloneEnv);

            result.Add("result", "success");
            result.Add("data", JToken.FromObject(cloneEnv));
            result.Add("message", $"New clone Test Environment: {testEnvCloneData.NewName}");

            return result;
        }
        public async Task<JObject> UpdateTestEnvAsync(TestEnvHistory testEnvHistory)
        {
            // Update test env
            JObject result = new JObject();
            TestEnv te = testEnvHistory.TestEnv;
            UpdateTestEnvData updateData = testEnvHistory.UpdateTestEnvData;
            testEnvHistory.UpdateTestEnvData.UpdateDate = DateTime.UtcNow;
            te.LastModifiedDate = DateTime.UtcNow;
            te.LastModifiedUser = updateData.UpdateBy;
            te.LastModifiedMessage = updateData.UpdateMessage;
            var filter = Builders<TestEnv>.Filter.Eq("_id", ObjectId.Parse(te.Id));
            var testEnv = _testenvironments.Find(filter).FirstOrDefault();
            if (testEnv == null)
            {
                result.Add("message", $"Can't find test env with id {te.Id}");
                result.Add("result", "error");
                return result;
            }
            await _testenvironments.ReplaceOneAsync(filter, te);

            // Create new record in collection TestEnvHistory
            await _testenvhistories.InsertOneAsync(testEnvHistory);

            result.Add("message", "Update test environment !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(te));
            return result;
        }
        public async Task<JObject> NewTestEnv(TestEnv testEnv)
        {
            // New test env
            JObject result = new JObject();
            var existingTestEnv = await _testenvironments.Find<TestEnv>(tv => tv.Name == testEnv.Name).FirstOrDefaultAsync();
            if (existingTestEnv != null)
            {
                result.Add("result", "error");
                result.Add("message", $"The Test Environment Name: {testEnv.Name} is taken, choose another one.");
                return result;
            }
            testEnv.LastModifiedDate = DateTime.UtcNow;
            testEnv.LastModifiedMessage = "New";
            testEnv.Nodes = new List<TestEnvNode>();

            // Create new record in collection TestEnvHistory
            await _testenvironments.InsertOneAsync(testEnv);
            result.Add("message", "Create new test environment !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(testEnv));
            return result;
        }
        public async Task<JObject> GetTestEnv(string id)
        {
            // New test env
            JObject result = new JObject();
            var existingTestEnv = await _testenvironments.Find<TestEnv>(tv => tv.Id == id).FirstOrDefaultAsync();
            if (existingTestEnv == null)
            {
                result.Add("result", "error");
                result.Add("message", $"The Test Environment with Id: {id} is not found.");
                return result;
            }

            result.Add("message", "Get test environment !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(existingTestEnv));
            return result;
        }
        public async Task<JObject> DeleteTestEnv(TestEnvHistory testEnvHistory)
        {
            // Delete test env
            JObject result = new JObject();
            var existingTestEnv = await _testenvironments.Find<TestEnv>(tv => tv.Id == testEnvHistory.TestEnv.Id).FirstOrDefaultAsync();
            if (existingTestEnv == null)
            {
                result.Add("result", "error");
                result.Add("message", $"The Test Environment with Id: {testEnvHistory.Id} is not found.");
                return result;
            }

            TestEnv te = testEnvHistory.TestEnv;
            UpdateTestEnvData updateData = testEnvHistory.UpdateTestEnvData;
            testEnvHistory.UpdateTestEnvData.UpdateDate = DateTime.UtcNow;
            te.LastModifiedDate = DateTime.UtcNow;
            te.LastModifiedUser = updateData.UpdateBy;
            te.LastModifiedMessage = updateData.UpdateMessage;
            te.IsDeleted = true;
            te.Name = $"{te.Name}_deleted";
            var filter = Builders<TestEnv>.Filter.Eq("_id", ObjectId.Parse(te.Id));
            var testEnv = _testenvironments.Find(filter).FirstOrDefault();
            if (testEnv == null)
            {
                result.Add("message", $"Can't find test env with id {te.Id}");
                result.Add("result", "error");
                return result;
            }

            await _testenvironments.ReplaceOneAsync(filter, te);

            // Create new record in collection TestEnvHistory
            await _testenvhistories.InsertOneAsync(testEnvHistory);

            result.Add("message", "Delete test environment !");
            result.Add("result", "success");
            result.Add("data", JToken.FromObject(te));
            return result;
        }
        public async Task<JObject> GetUpdateHistories(string testEnvId)
        {
            JObject result = new JObject();
            var filter = Builders<TestEnvHistory>.Filter.Eq(te => te.TestEnv.Id, testEnvId);
            var sort = Builders<TestEnvHistory>.Sort.Descending(te => te.UpdateTestEnvData.UpdateDate);
            var testEnvHistories = await _testenvhistories.Find(filter).Sort(sort).ToListAsync();
            if (testEnvHistories == null)
            {
                result.Add("data", "No histories!");
                result.Add("result", "success");
                result.Add("count", 0);
            }
            else
            {
                result.Add("data", JToken.FromObject(testEnvHistories));
                result.Add("result", "success");
                result.Add("count", testEnvHistories.Count);
            }

            return result;
        }
    }
}