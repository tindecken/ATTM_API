using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    public class TestCase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; } 
        public TestType Type { get; set; } = 0;
        public bool isPrimary { get; set; } = false;
        public bool isDisabled { get; set; } = false;
        public bool isDeleted { get; set; } = false;
        public string WorkItem { get; set; } = string.Empty;
        public User Owner { get; set; }
        public DateTime createdDate { get; set; } = DateTime.UtcNow;
        public DateTime lastUpdatedDate { get; set; } = DateTime.UtcNow;
        public TestStatus lastRunningStatus { get; set; } = 0;
        public User lastUpdatedUser { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TimeOutInMinutes { get; set; } = 60;
        public TestCase DependOn { get; set; }
        public List<string> TestSteps { get; set; } = new List<string>();

    }
}