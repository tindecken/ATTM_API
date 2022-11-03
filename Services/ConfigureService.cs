using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using CommonModels;
using System.Net;

namespace ATTM_API.Services
{
    public class ConfigureService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IATTMDatabaseSettings _dbSettings;
        private readonly IATTMAppSettings _appSettings;

        public ConfigureService(IATTMAppSettings appSettings, IATTMDatabaseSettings dbSettings)
        {
            _appSettings = appSettings;
            _dbSettings = dbSettings;
        }

        public async Task<JObject> Get() {
            JObject result = new JObject();
            JObject settings = new JObject {
                { "dbSettings", JToken.FromObject(_dbSettings) },
                { "appSettings", JToken.FromObject(_appSettings) }
            };

            result.Add("data", settings);

            result.Add("result", "success");
            result.Add("message", $"Success");
            return result;
        }
        
    }
}