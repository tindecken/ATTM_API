using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class RegressionRunRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Status { get; set; }
        public string Log { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int ExecuteTime { get; set; }
        public string RunMachine { get; set; }
        public string ErrorScreenshot { get; set; }
        public string Screenshot1 { get; set; }
        public string Screenshot2 { get; set; }
        public string Comment { get; set; }
        public Dictionary<string, string> Buffers { get; set; } = new Dictionary<string, string>();
    }
}