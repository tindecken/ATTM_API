using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestClient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IPAddress { get; set; }
        public string Type { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string RegressionFolder { get; set; }
        public string DevelopFolder { get; set; }
        public string RunnerFolder { get; set; }
        public string Status { get; set; }

        public string DeploySourceMessage { get; set; }
        public string DeploySourceStatus { get; set; }
        public string UpdateReleaseMessage { get; set; }
        public string UpdateReleaseStatus { get; set; }
    }
}