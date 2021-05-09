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
        private readonly IMongoCollection<RegressionTest> _regressionTests;

        public RegressionService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _regressions = database.GetCollection<Regression>(settings.RegressionsCollectionName);
            _regressionTests = database.GetCollection<RegressionTest>(settings.RegressionTestsCollectionName);
        }

        public List<Regression> Get() =>
            _regressions.Find(new BsonDocument()).ToList();
            

        public async Task<Regression> Get(string id) =>
            await _regressions.Find<Regression>(regression => regression.Id == id).FirstOrDefaultAsync();

        public async Task<Regression> Create(Regression regression)
        {
            try
            {
                var existingReg = await _regressions.Find<Regression>(r => r.Name == regression.Name).FirstOrDefaultAsync();
                if (existingReg == null)
                {
                    await _regressions.InsertOneAsync(regression);
                    return regression;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<JObject> AddTestToRegression(string regressionId, RegressionTest regressionTest)
        {
            // Get regression
            JObject result = new JObject();
            var currRegression = await _regressions.Find<Regression>(r => r.Id == regressionId).FirstOrDefaultAsync();
            if (currRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with ID: {regressionId}");
                return result;
            }

            // Already added but with Id --> not delete it.
            if (currRegression.RegressionTestIds.Contains(regressionTest.Id))
            {
                result.Add("result", "error");
                result.Add("message", $"RegressionTest {regressionTest.Id} is already exist");
                return result;
            }

            // get all RegressionTest and compare with input RegressionTest
            bool isExisted = false;
            foreach (var regressionTestId in currRegression.RegressionTestIds)
            {
                var regTest = await _regressionTests.Find<RegressionTest>(r => r.Id == regressionTestId)
                    .FirstOrDefaultAsync();
                if (regTest.TestCaseCodeName.Equals(regressionTest.TestCaseCodeName)) isExisted = true;
            }

            // Already added but with CodeName --> delete it.
            if (isExisted)
            {
                await _regressionTests.FindOneAndDeleteAsync(rt => rt.Id == regressionTest.Id);
                result.Add("result", "error");
                result.Add("message", $"RegressionTest {regressionTest.TestCaseCodeName} is already exist, deleted it.");
                return result;
            }

            // Update RegressionTest, set Release, Build
            var regressionTestUpdate = Builders<RegressionTest>.Update.Set(r => r.Release, currRegression.Release)
                .Set(r => r.Build, currRegression.Build);
            await _regressionTests.FindOneAndUpdateAsync(r => r.Id == regressionTest.Id, regressionTestUpdate);

            // Add to regression
            var filter = Builders<Regression>.Filter.Eq(r => r.Id, regressionId);
            var update = Builders<Regression>.Update.Push(r => r.RegressionTestIds, regressionTest.Id);
            await _regressions.FindOneAndUpdateAsync(filter, update);
            
            result.Add("result", "success");
            result.Add("message", $"Added {regressionTest.TestCaseCodeName}");
            return result;
        }
    }
}