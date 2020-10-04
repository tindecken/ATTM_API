using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    public class Keyword
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; } 
        public User Owner { get; set; }
        public DateTime createdDate { get; set; } = DateTime.UtcNow;
        public DateTime lastUpdatedDate { get; set; } = DateTime.UtcNow;
        public User lastUpdatedUser { get; set; }
        public string UpdatedMessage { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<TestParam> Params { get; set; } = new List<TestParam>();
        public string ImageURL { get; set; }
        public string WorkItem { get; set; }
        public string Category { get; set; }
        public string Feature { get; set; }
        public bool isDisabled { get; set; } = false;
        public TestAUTType AUTType { get; set; } = 0;

    }
}