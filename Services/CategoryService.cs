using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATTM_API.Services
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categories;
        private readonly IMongoCollection<TestSuite> _testsuites;

        public async CategoryService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            var options = new CreateIndexOptions() { Unique = true };
            var field = new StringFieldDefinition<Category>("name");
            var indexDefinition = new IndexKeysDefinitionBuilder<Category>().Ascending(field);

            var indexModel = new CreateIndexModel<Category>(indexDefinition,options);
            await database.GetCollection<Category>(settings.TestSuitesCollectionName).Indexes.CreateOneAsync(indexModel);


            _testsuites = await database.GetCollection<TestSuite>(settings.TestSuitesCollectionName);
        }

        public List<Category> Get() =>
            _categories.Find(new BsonDocument()).ToList();
            

        public Category Get(string id) =>
            _categories.Find<Category>(category => category.Id == id).FirstOrDefault();

        public Category Create(Category category)
        {
            var existingCat = _categories.Find<Category>(cat => cat.CategoryName == category.CategoryName).FirstOrDefault();
            if(existingCat == null) {
                _categories.InsertOne(category);
                return category;
            }else{
                return null;
            }
        }

        public TestSuite CreateTestSuite(string catId, TestSuite ts)
        {
            //Check testsuite is already exist or not
            var existingTS = _testsuites.Find<TestSuite>(t => t.TestSuiteName == ts.TestSuiteName).FirstOrDefault();
            if(existingTS != null){
                return null;
            }else {
                _testsuites.InsertOne(ts);
                var filter = Builders<Category>.Filter.Eq(cat => cat.Id, catId);
                var update = Builders<Category>.Update.Push<string>("_id_testSuites", ts.Id);

                _categories.FindOneAndUpdate(filter, update);
                return ts;
            }
            
        }

        public void Update(string id, Category categoryIn) =>
            _categories.ReplaceOne(category => category.Id == id, categoryIn);

        public void Remove(Category categoryIn) =>
            _categories.DeleteOne(category => category.Id == categoryIn.Id);

        public void Remove(string id) => 
            _categories.DeleteOne(category => category.Id == id);
    }
}