using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;

        public CategoryService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _categories = database.GetCollection<Category>(settings.CategoriesCollectionName);
            _testsuites = database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
        }

        public List<Category> Get() =>
            _categories.Find(new BsonDocument()).ToList();
            

        public async Task<Category> Get(string id) =>
            await _categories.Find<Category>(category => category.Id == id).FirstOrDefaultAsync();

        public async Task<Category> Create(Category category)
        {
            try {
                var existingCat = await _categories.Find<Category>(cat => cat.Name == category.Name).FirstOrDefaultAsync();
                if(existingCat == null) {
                    await _categories.InsertOneAsync(category);
                    return category;
                }else{
                    return null;
                }
            } catch (Exception ex) {
                throw ex;   
            }
            
        }

        public async Task<TestSuite> CreateTestSuite(string catId, TestSuite ts)
        {
            try
            {
                var existingTS = await _testsuites.Find<TestSuite>(t => t.Name == ts.Name).FirstOrDefaultAsync();
                if(existingTS != null){
                    return null;
                }else {
                    await _testsuites.InsertOneAsync(ts);
                    var filter = Builders<Category>.Filter.Eq(cat => cat.Id, catId);
                    var update = Builders<Category>.Update.Push<string>(cat => cat.TestSuites, ts.Id);
                    await _categories.FindOneAndUpdateAsync(filter, update);
                    return ts;
                }
            }
            catch (Exception ex)
            {
                throw ex;   
            }
        }

        public async Task<JObject> GetAllAsync()
        {
            
        }

        public void Update(string id, Category categoryIn) =>
            _categories.ReplaceOne(category => category.Id == id, categoryIn);

        public void Remove(Category categoryIn) =>
            _categories.DeleteOne(category => category.Id == categoryIn.Id);

        public void Remove(string id) => 
            _categories.DeleteOne(category => category.Id == id);
    }
}