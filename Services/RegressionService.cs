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
        public async Task<JObject> AddTestToRegression(string regressionId, string regressionTestId)
        {
            // Get regression
            JObject result = new JObject();
            var currRegression = await _regressions.Find<Regression>(r => r.Id == regressionId).FirstOrDefaultAsync();
            if (currRegression == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Regression with ID: {regressionId}");
                result.Add("data", null);
                return result;
            }

            var currRegressionTest = await _regressionTests.Find<RegressionTest>(rt => rt.Id == regressionTestId).FirstOrDefaultAsync();
            if (currRegressionTest == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found RegressionTest with ID: {regressionTestId}");
                result.Add("data", null);
                return result;
            }

            if (currRegression.RegressionTestIds != null)
            {
                // Already added but with Id --> not delete it.
                if (currRegression.RegressionTestIds.Contains(regressionTestId))
                {
                    result.Add("result", "success");
                    result.Add("message", $"RegressionTestId {regressionTestId} is already existed");
                    result.Add("data", null);
                    return result;
                }

                // get all RegressionTest and compare with input RegressionTest
                bool isExisted = false;
                foreach (var rtId in currRegression.RegressionTestIds)
                {
                    var regTest = await _regressionTests.Find<RegressionTest>(r => r.Id == rtId)
                        .FirstOrDefaultAsync();
                    if (regTest.TestCaseCodeName.Equals(currRegressionTest.TestCaseCodeName)) isExisted = true;
                }

                // Already added but with CodeName --> delete it.
                if (isExisted)
                {
                    await _regressionTests.FindOneAndDeleteAsync(rt => rt.Id == regressionTestId);
                    result.Add("result", "error");
                    result.Add("message", $"RegressionTest {currRegressionTest.TestCaseCodeName} is already existed, deleted it.");
                    result.Add("data", null);
                    return result;
                }
            }

            // Update RegressionTest, set Release, Build
            var regressionTestUpdate = Builders<RegressionTest>.Update.Set(r => r.Release, currRegression.Release)
                .Set(r => r.Build, currRegression.Build);
            await _regressionTests.FindOneAndUpdateAsync(r => r.Id == regressionTestId, regressionTestUpdate);

            // Add to regression
            var filter = Builders<Regression>.Filter.Eq(r => r.Id, regressionId);
            var update = Builders<Regression>.Update.Push(r => r.RegressionTestIds, regressionTestId);
            await _regressions.FindOneAndUpdateAsync(filter, update);
            
            result.Add("result", "success");
            result.Add("message", $"Added {currRegressionTest.TestCaseCodeName}");
            result.Add("data", JToken.FromObject(currRegressionTest));
            return result;
        }
    }
}