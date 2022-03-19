using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestEnv
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [JsonRequired]
        public string Name { get; set; } 
        public string Description { get; set; } = string.Empty;
        [JsonRequired]
        public List<TestEnvNode> Nodes { get; set; } = new List<TestEnvNode>();
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
        public string LastModifiedMessage { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
    public class TestEnvNode
    {
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}