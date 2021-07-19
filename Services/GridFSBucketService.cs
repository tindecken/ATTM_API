using ATTM_API.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.GridFS;

namespace ATTM_API.Services
{
    public class GridFSBucketService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(Program));
        private readonly IGridFSBucket _bucket;

        public GridFSBucketService(IATTMDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _bucket = new GridFSBucket(database);
        }

        public async Task<JObject> Get(string Id)
        {
            JObject result = new JObject();
            var bytes = await _bucket.DownloadAsBytesAsync(ObjectId.Parse(Id));
            if (bytes != null)
            {
                string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                result.Add("result", "success");
                result.Add("data", "data:image/png;base64," + base64String);
            }
            else
            {
                result.Add("result", "error");
                result.Add("data", null);
            }

            return result;
        }
    }
}