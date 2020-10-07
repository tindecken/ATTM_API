using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestEnvironment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string TestEnvironmentName { get; set; } 
        public string Description { get; set; } = string.Empty;
        public List<TestEnvCategory> TestEnvCategories { get; set; } = new List<TestEnvCategory>();
    }
    public class TestEnvCategory 
    {
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<TestEnvNode> TestEnvNodes { get; set; } = new List<TestEnvNode>();
    }
    public class TestEnvNode {
        public string Name { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}