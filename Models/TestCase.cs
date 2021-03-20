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
        [BsonElement("Name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; } 
        public string CodeName { get; set; }
        public string TestCaseType { get; set; }   
        public TestStatus LastRunningStatus { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public string WorkItem { get; set; } = string.Empty;
        public string Designer { get; set; }
        public string Team { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        public User LastModifiedUser { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TimeOutInMinutes { get; set; } = 60;
        public TestCase DependOn { get; set; }
        public List<TestStep> TestSteps { get; set; } = new List<TestStep>();
        public string CategoryId { get; set; }
        public string TestSuiteId { get; set; }
        public string  TestGroupId { get; set; }
    }
}