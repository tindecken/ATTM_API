using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class TestEnvironmentService
    {
        private readonly IMongoCollection<TestEnvironment> _testenvironments;

        public TestEnvironmentService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _testenvironments = database.GetCollection<TestEnvironment>(settings.TestEnvironmentsCollectionName);
        }

        public List<TestEnvironment> Get() =>
            _testenvironments.Find(new BsonDocument()).ToList();
            

        public async Task<TestEnvironment> Get(string id) =>
            await _testenvironments.Find<TestEnvironment>(tv => tv.Id == id).FirstOrDefaultAsync();
        
        public async Task<TestEnvironment> Create(TestEnvironment testEnv) {
            try {
                var existingEnv = await _testenvironments.Find<TestEnvironment>(tv => tv.TestEnvironmentName == testEnv.TestEnvironmentName).FirstOrDefaultAsync();
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

    }
}