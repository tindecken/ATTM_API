using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ATTM_API.Models
{
    [BsonIgnoreExtraElements]
    public class TestSuite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Name")]
        [JsonRequired]
        [JsonProperty("Name")]
        public string Name { get; set; }
        public string CodeName { get; set; }
        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("WorkItem")]
        public string WorkItem { get; set; } = string.Empty;
        [BsonElement("TestGroupIds")]
        public List<string> TestGroupIds { get; set; } = new List<string>();
        public string CategoryId { get; set; }

    }
}