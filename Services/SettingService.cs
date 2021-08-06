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
    public class SettingService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IMongoCollection<Setting> _settings;

        public SettingService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _settings = database.GetCollection<Setting>(settings.SettingsCollectionName);
        }

        public List<Setting> Get() =>
            _settings.Find(new BsonDocument()).ToList();
            

        public async Task<Setting> Get(string id) =>
            await _settings.Find<Setting>(setting => setting.Id == id).FirstOrDefaultAsync();

        public async Task<Setting> Create(Setting setting)
        {
            try
            {
                var existingSetting = await _settings.Find<Setting>(s => s.Name == setting.Name).FirstOrDefaultAsync();
                if (existingSetting == null)
                {
                    await _settings.InsertOneAsync(setting);
                    return setting;
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

        public async Task<JObject> Update(string settingId, Setting setting)
        {
            JObject result = new JObject();
            var existingSetting = await _settings.Find<Setting>(s => s.Id == settingId).FirstOrDefaultAsync();
            if (existingSetting == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Setting with ID: {settingId}");
                result.Add("data", null);
                return result;
            }

            setting.Id = existingSetting.Id;
            setting.UpdatedDateTime = DateTime.UtcNow;
            var replaced = await _settings.FindOneAndReplaceAsync(s => s.Id == settingId, setting);
            if (replaced != null)
            {
                result.Add("result", "success");
                result.Add("message", $"Update setting success !");
                result.Add("data", JToken.FromObject(setting));
            }
            else
            {
                result.Add("result", "error");
                result.Add("message", $"An error occurs while update setting {settingId}");
                result.Add("data", null);
            }

            return result;
        }

        public async Task<JObject> GetSettingByName(string Name)
        {
            JObject result = new JObject();
            var existingSetting = await _settings.Find<Setting>(s => s.Name == Name).FirstOrDefaultAsync();
            if (existingSetting == null)
            {
                result.Add("result", "error");
                result.Add("message", $"Not Found Setting with Name: {Name}");
                result.Add("data", null);
                return result;
            }

            result.Add("result", "success");
            result.Add("message", $"Success get getting with Name: {Name}");
            result.Add("data", JToken.FromObject(existingSetting));

            return result;
        }
    }
}