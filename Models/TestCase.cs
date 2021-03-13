using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestCase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; } 
        public string tcId { get; set; }
        public string TestCaseType { get; set; }   
        public TestStatus lastRunningStatus { get; set; } = 0;
        public bool isPrimary { get; set; } = false;
        public bool isDisabled { get; set; } = false;
        public bool isDeleted { get; set; } = false;
        public string WorkItem { get; set; } = string.Empty;
        public string Designer { get; set; }
        public string Team { get; set; }
        public DateTime createdDate { get; set; } = DateTime.UtcNow;
        public DateTime lastModifiedDate { get; set; } = DateTime.UtcNow;
        
        public User lastModifiedUser { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TimeOutInMinutes { get; set; } = 60;
        public TestCase DependOn { get; set; }
        public List<TestStep> TestSteps { get; set; } = new List<TestStep>();
        public string CategoryName { get; set; }
        public string TestSuiteId { get; set; } // not mongoId
        public string  TestGroupId { get; set; } // not a mongoId
    }
}