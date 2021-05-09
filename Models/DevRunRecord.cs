using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class DevRunRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement]
        [JsonRequired]
        public string TestCaseId { get; set; }
        [JsonRequired]
        public string TestCaseName { get; set; }
        public string Description { get; set; }
        public string TestCaseType { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string TestSuite { get; set; }
        public string TestGroup { get; set; }
        public string Team { get; set; }
        public string ErrorMessage { get; set; }
        public string Log { get; set; }
        public DateTime StartAt { get; set; }
        public string WorkItem { get; set; }
        public DateTime EndAt { get; set; }
        public int ExecuteTime { get; set; }
        public string RunMachine { get; set; }
        public string ErrorScreenshot { get; set; }
        public string ErrorTearDownScreenshot { get; set; }
        public string Screenshot1 { get; set; }
        public string Screenshot2 { get; set; }
        public string Comments { get; set; }
    }
}
